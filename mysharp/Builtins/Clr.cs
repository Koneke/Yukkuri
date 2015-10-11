using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace mysharp.Builtins.Clr
{
	public static class ClrTools
	{
		public static Type GetType(
			mysState state,
			string type
		) {
			// add fallback stack of some kind here.
			// like, say our fallback stack is [System, Mentonauts].
			// then, if our normal lookup fails, we check if the top
			// of the stack has such a type (e.g. Mentonauts.Test),
			// then go to the next (System.Test), etc.
			// lets us be a bit less Java.

			foreach( Assembly a in state.exposedAssemblies ) {
				Type foundType = a
					.GetExportedTypes()
					.FirstOrDefault( t => t.FullName == type )
				;

				if ( foundType != null ) {
					return foundType;
				}
			}

			throw new Exception( "Type not imported." );
		}

		// needs some heavy review/revision and looking over!
		public static mysToken ConvertClrObject(
			object obj
		) {
			if ( obj == null ) {
				return null;
			}

			return new mysToken( obj );
		}
	}

	public static class NewClrObject
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( typeof(Type) );

			f.Function = (args, state, sss) => {
				object result = Activator.CreateInstance(
					(Type)args[ 0 ].Value
				);

				return new List<mysToken>() {
					new mysToken( result )
				};
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "new", functionGroup, global );
		}
	}

	public static class ClrDot
	{
		static mysFunctionGroup functionGroup;

		static object get(
			object obj,
			string field
		) {
			object target = null;
			Type targetType = null;

			mysToken token = obj as mysToken;

			if ( token != null ) {
				if ( token.Type == typeof(Type) ) {
					target = null;
					targetType = (Type)token.Value;

				} else {
					target = token.Value;
					targetType = target.GetType();

				}
			} else {
				// if it's neither clrObject or clrType, it's just an
				// object, straight up, sent internally

				target = obj;
				targetType = target.GetType();
			}

			return targetType
				.GetField( field )
				.GetValue( target )
			;
		}

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f;

			// simple variant

			f = new mysBuiltin();

			f.Signature.Add( typeof(CLR) );
			f.Signature.Add( typeof(mysSymbol) );

			f.Function = (args, state, sss) => {
				string field = 
					(args[ 1 ].Value as mysSymbol)
					.StringRepresentation
				;

				return new List<mysToken>() {
					ClrTools.ConvertClrObject(
						get( args[ 0 ], field )
					)
				};
			};

			functionGroup.Variants.Add( f );

			// list variant

			f = new mysBuiltin();

			f.Signature.Add( typeof(CLR) );
			f.Signature.Add( typeof(List<mysToken>) );

			f.Function = (args, state, sss) => {
				List<mysToken> list = (List<mysToken>)args[ 1 ].Value;

				if (
					list == null ||
					!list.All( t => t.Type == typeof(mysSymbol) )
				) {
					throw new ArgumentException();
				}

				List<mysSymbol> chain = list
					.Select( t => t.Value as mysSymbol)
					.ToList();

				object c = args[ 0 ];
				for ( int i = 0; i < chain.Count; i++ ) {
					string field =
						(chain[ i ] as mysSymbol)
						.StringRepresentation
					;

					c = get( c, field );
				}

				return new List<mysToken>() {
					ClrTools.ConvertClrObject( c )
				};
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( ".", functionGroup, global );
		}
	}

	// should probably be remade to get type by string, if it is to be kept at
	// all, since we have the direct #<typename> syntax now.
	public static class ClrType
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( typeof(mysSymbol) );

			f.Function = (args, state, sss) => {
				string field =
					(args[ 0 ].Value as mysSymbol)
					.StringRepresentation
				;

				return new List<mysToken>() {
					new mysToken(
						ClrTools.GetType( state, field )
					)
				};
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "#type", functionGroup, global );
		}
	}

	public static class Set
	{
		public static void Setup( mysSymbolSpace global ) {
			mysBuiltin f;

			f = new mysBuiltin();

			f.Signature.Add( typeof(CLR) );
			// we don't *actually* want this as a symbol though,
			// it's just the field name really
			f.Signature.Add( typeof(mysSymbol) );
			f.Signature.Add( typeof(ANY) );

			f.Function = (args, state, sss) => {
				mysToken clrThing = args[ 0 ];
				string field =
					(args[ 1 ].Value as mysSymbol)
					.StringRepresentation
				;
				mysToken newValue = args[ 2 ];

				object value = newValue.Value;

				Type targetType;
				object targetObject = null;

				if ( clrThing.Type == typeof(Type) ) {
					targetObject = null;
					targetType = (Type)clrThing.Value;
				} else {
					targetObject = clrThing.Value;
					targetType = targetObject.GetType();
				}

				FieldInfo fi = targetType.GetField( field );

				value = Convert.ChangeType( value, fi.FieldType );

				targetType
					.GetField( field )
					.SetValue( targetObject, value )
				;

				return null;
			};

			mysBuiltin.AddVariant( "set", f, global );
		}
	}
}
