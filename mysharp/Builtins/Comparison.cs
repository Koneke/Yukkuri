using System.Collections.Generic;

namespace mysharp.Builtins.Comparison
{
	public static class Equals
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( typeof(NUMBER) );
			f.Signature.Add( typeof(NUMBER) );

			f.Function = (args, state, sss) => {
				return new List<mysToken>() {
					new mysToken(
						NUMBER.Promote( args[ 0 ] ) ==
						NUMBER.Promote( args[ 1 ] )
					)
				};
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "=", functionGroup, global );
		}
	}

	public static class GreaterThan
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( typeof(NUMBER) );
			f.Signature.Add( typeof(NUMBER) );

			f.Function = (args, state, sss) => {
				return new List<mysToken>() {
					new mysToken(
						NUMBER.Promote( args[ 0 ] ) >
						NUMBER.Promote( args[ 1 ] )
					)
				};
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( ">", functionGroup, global );
		}
	}
}
