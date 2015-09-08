using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
			Type t = obj.GetType();

			if ( t == typeof(int) ) {
				return new mysIntegral( (int)obj );
			}

			throw new NotImplementedException();
		}
	}

	public static class NewClrObject
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( mysTypes.Symbol );

			f.Function = (args, state, sss) => {
				string name = (args[ 0 ] as mysSymbol ).StringRepresentation;

				foreach( Assembly a in state.exposedAssemblies ) {
					if ( a.GetExportedTypes()
						.Any( t => t.FullName == name )
					) {
						return new clrObject(
							Activator.CreateInstance(
								ClrTools.GetType( state, name )
							)
						);
					}
				}

				throw new Exception( "Type not imported." );
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
}
