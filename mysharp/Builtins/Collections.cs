using System.Collections.Generic;

namespace mysharp.Builtins.Collections
{
	public static class Range
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();
			mysBuiltin f;

			f = new mysBuiltin();

			f.Signature.Add( typeof(int) );

			f.Function = (args, state, sss) => {
				List<int> range = new List<int>();

				for ( int i = 1; i <= (int)args[ 0 ].Value; i++ ) {
					range.Add( i );
				}

				return new List<mysToken>() {
					new mysToken( range ).Quote()
				};
			};

			functionGroup.Variants.Add( f );

			f = new mysBuiltin();

			f.Signature.Add( typeof(int) );
			f.Signature.Add( typeof(int) );

			f.Function = (args, state, sss) => {
				List<mysToken> range = new List<mysToken>();

				for (
					long i = (long)args[ 0 ].Value;
					i <= (long)args[ 1 ].Value;
					i++
				) {
					range.Add( new mysToken( (int)i ) );
				}

				return new List<mysToken>() {
					new mysToken( range )
				};
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "range", functionGroup, global );
		}
	}
}
