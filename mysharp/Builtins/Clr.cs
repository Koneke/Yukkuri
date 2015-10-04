﻿using System;
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
			foreach( Assembly a in state.exposedAssemblies ) {
				if ( a.GetExportedTypes()
					.Any( t => t.FullName == type )
				) {
					return a.GetType( type );
				}
			}

			throw new Exception( "Type not imported." );
		}

		public static mysToken ConvertClrObject(
			object obj
		) {
			if ( obj == null ) {
				return null;
			}

			Type t = obj.GetType();

			if (
				t == typeof(int) ||
				t == typeof(long)
			) {
				return new mysToken( (int)obj );
			}

			if (
				t == typeof(string)
			) {
				return new mysToken( (string)obj );
			}

			if (
				t == typeof(bool)
			) {
				return new mysToken( (bool)obj );
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
					(Type)args[ 0 ].InternalValue
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

		static object get( object obj, mysToken field ) {
			if ( field.Type != mysTypes.Symbol ) {
				throw new ArgumentException();
			}

			object target = null;
			Type targetType = null;

			mysToken token = obj as mysToken;

			if ( token != null ) {
				if ( token.Type == mysTypes.clrObject ) {
					target = token.InternalValue;
					targetType = target.GetType();
				} else if ( token.Type == mysTypes.clrType ) {
					target = null;
					targetType = (Type)token.InternalValue;
				}
			} else {
				// if it's neither clrObject or clrType, it's just an
				// object, straight up, sent internally

				target = obj;
				targetType = target.GetType();
			}

			return targetType
				.GetField( (field as mysSymbol).StringRepresentation )
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
				return new List<mysToken>() {
					ClrTools.ConvertClrObject(
						get( args[ 0 ], args[ 1 ] )
					)
				};
			};

			functionGroup.Variants.Add( f );

			// list variant

			f = new mysBuiltin();

			f.Signature.Add( typeof(CLR) );
			f.Signature.Add( typeof(mysList) );

			f.Function = (args, state, sss) => {
				mysList list = args[ 1 ] as mysList;

				if (
					list == null ||
					!list.InternalValues.All( t => t.Type == mysTypes.Symbol )
				) {
					throw new ArgumentException();
				}

				List<mysSymbol> chain = list.InternalValues
					.Select( t => t as mysSymbol)
					.ToList();

				object c = args[ 0 ];
				for ( int i = 0; i < chain.Count; i++ ) {
					c = get( c, chain[ i ] );
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
				mysSymbol symbol = args[ 0 ] as mysSymbol;

				return new List<mysToken>() {
					new mysToken(
						ClrTools.GetType( state, symbol.StringRepresentation )
					)
				};
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "#type", functionGroup, global );
		}
	}

	public static class Call
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f;

			// static

			f = new mysBuiltin();

			f.Signature.Add( typeof(mysSymbol) );
			f.Signature.Add( typeof(Type) );
			f.Signature.Add( typeof(mysList) );

			f.Function = (args, state, sss) => {
				mysSymbol symbol = args[ 0 ] as mysSymbol;
				mysToken type = args[ 1 ];
				mysList argsList = args[ 2 ] as mysList;

				Type[] argumentTypes = argsList.InternalValues
					.Select( t => t.InternalValue.GetType() )
					.ToArray();

				object[] arguments = argsList.InternalValues
					.Select( t => t.InternalValue )
					.ToArray();

				MethodInfo mi = ((Type)type.InternalValue).GetMethod(
					symbol.StringRepresentation,
					argumentTypes
				);

				object result = mi.Invoke(
					null,
					arguments
				);

				return new List<mysToken>() {
					ClrTools.ConvertClrObject( result )
				};
			};

			functionGroup.Variants.Add( f );

			// instance

			f = new mysBuiltin();

			f.Signature.Add( typeof(mysSymbol) );
			f.Signature.Add( typeof(object) );
			f.Signature.Add( typeof(mysList) );

			f.Function = (args, state, sss) => {
				mysSymbol symbol = args[ 0 ] as mysSymbol;
				mysToken obj = args[ 1 ];
				mysList argsList = args[ 2 ] as mysList;

				Type[] argumentTypes = argsList.InternalValues
					.Select( t => t.InternalValue.GetType() )
					.ToArray();

				object[] arguments = argsList.InternalValues
					.Select( t => t.InternalValue )
					.ToArray();

				MethodInfo mi = obj.InternalValue.GetType().GetMethod(
					symbol.StringRepresentation,
					argumentTypes
				);

				object result = mi.Invoke(
					obj.InternalValue,
					arguments
				);

				return new List<mysToken>() {
					ClrTools.ConvertClrObject( result )
				};
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "#call", functionGroup, global );
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
				mysSymbol field = args[ 1 ] as mysSymbol;
				mysToken newValue = args[ 2 ];

				string fieldName = field.StringRepresentation;
				object value = newValue.InternalValue;

				Type targetType;
				object targetObject = null;

				if ( clrThing.RealType == typeof(Type) ) {
					targetObject = null;
					targetType = (Type)clrThing.InternalValue;
				} else {
					targetObject = clrThing.InternalValue;
					targetType = targetObject.GetType();
				}

				FieldInfo fi = targetType.GetField( fieldName );

				value = Convert.ChangeType( value, fi.FieldType );

				targetType
					.GetField( fieldName )
					.SetValue( targetObject, value )
				;

				return null;
			};

			mysBuiltin.AddVariant( "set", f, global );
		}
	}
}
