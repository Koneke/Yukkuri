using System;
using System.Linq;
using System.Reflection;

namespace mysharp.Builtins.Clr
{
	public static class NewClrObject
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( mysTypes.String );

			f.Function = (args, state, sss) => {
				mysString type = args[ 0 ] as mysString;

				foreach( Assembly a in state.exposedAssemblies ) {
					if ( a.GetExportedTypes()
						.Any( t => t.FullName == type.Value )
					) {
						return new clrObject(
							Activator.CreateInstance(
								a.GetType( type.Value )
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
}

