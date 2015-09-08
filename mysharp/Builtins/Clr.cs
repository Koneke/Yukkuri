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

			if ( t == typeof(int) ) {
				return new mysIntegral( (int)obj );
			}

			return new clrObject( obj );
		}
	}

	public static class NewClrObject
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( mysTypes.clrType );

			f.Function = (args, state, sss) => {
				Type type = (args[ 0 ] as clrType).Value;

				return new clrObject(
					Activator.CreateInstance( type )
				);
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "#new", functionGroup, global );
		}
	}

	public static class ClrDot
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( mysTypes.clrObject );
			f.Signature.Add( mysTypes.List );

			f.Function = (args, state, sss) => {
				clrObject instance = args[ 0 ] as clrObject;
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

				object current = instance.Value;

				for ( int i = 0; i < chain.Count; i++ ) {
					current = current
						.GetType()
						.GetField( chain[ i ].StringRepresentation )
						.GetValue( current )
					;
				}

				return ClrTools.ConvertClrObject( current );
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( ".", functionGroup, global );
		}
	}

	public static class ClrType
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( mysTypes.Symbol );

			f.Function = (args, state, sss) => {
				mysSymbol symbol = args[ 0 ] as mysSymbol;

				return new clrType(
					ClrTools.GetType( state, symbol.StringRepresentation )
				);
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

			f.Signature.Add( mysTypes.Symbol );
			f.Signature.Add( mysTypes.clrType );
			f.Signature.Add( mysTypes.List );

			f.Function = (args, state, sss) => {
				mysSymbol symbol = args[ 0 ] as mysSymbol;
				clrType type = args[ 1 ] as clrType;
				mysList argsList = args[ 2 ] as mysList;

				Type[] argumentTypes = argsList.InternalValues
					.Select( t => t.InternalValue.GetType() )
					.ToArray();

				object[] arguments = argsList.InternalValues
					.Select( t => t.InternalValue )
					.ToArray();

				MethodInfo mi = type.Value.GetMethod(
					symbol.StringRepresentation,
					argumentTypes
				);

				object result = mi.Invoke(
					null,
					arguments
				);

				return ClrTools.ConvertClrObject( result );
			};

			functionGroup.Variants.Add( f );

			// instance

			f = new mysBuiltin();

			f.Signature.Add( mysTypes.Symbol );
			f.Signature.Add( mysTypes.clrObject );
			f.Signature.Add( mysTypes.List );

			f.Function = (args, state, sss) => {
				mysSymbol symbol = args[ 0 ] as mysSymbol;
				clrObject obj = args[ 1 ] as clrObject;
				mysList argsList = args[ 2 ] as mysList;

				Type[] argumentTypes = argsList.InternalValues
					.Select( t => t.InternalValue.GetType() )
					.ToArray();

				object[] arguments = argsList.InternalValues
					.Select( t => t.InternalValue )
					.ToArray();

				MethodInfo mi = obj.Value.GetType().GetMethod(
					symbol.StringRepresentation,
					argumentTypes
				);

				object result = mi.Invoke(
					obj.Value,
					arguments
				);

				return ClrTools.ConvertClrObject( result );
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "#call", functionGroup, global );
		}
	}
}
