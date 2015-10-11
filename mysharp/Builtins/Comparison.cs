using System.Collections.Generic;

namespace mysharp.Builtins.Comparison
{
	public static class Equals
	{
		static mysFunctionGroup functionGroup;

		static bool CompareNumbers( mysToken a, mysToken b ) {
			return
				NUMBER.Promote( a ) ==
				NUMBER.Promote( b )
			;
		}

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( typeof(NUMBER) );
			f.Signature.Add( typeof(NUMBER) );

			f.Function = (args, state, sss) => {
				return new List<mysToken>() {
					new mysToken(
						CompareNumbers( args[ 0 ], args[ 1 ] )
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
				mysToken first = mysToken.PromoteToFloat( args[ 0 ] );
				mysToken second = mysToken.PromoteToFloat( args[ 1 ] );

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

