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

			f.Signature.Add( mysTypes.Integral );

			f.Function = (args, state, sss) => {
				List<mysToken> range = new List<mysToken>();

				for ( long i = 1; i <= (long)args[ 0 ].InternalValue; i++ ) {
					range.Add( new mysIntegral( i ) );
				}

				return range;
			};

			functionGroup.Variants.Add( f );

			f = new mysBuiltin();

			f.Signature.Add( mysTypes.Integral );
			f.Signature.Add( mysTypes.Integral );

			f.Function = (args, state, sss) => {
				List<mysToken> range = new List<mysToken>();

				for (
					long i = (long)args[ 0 ].InternalValue;
					i <= (long)args[ 1 ].InternalValue;
					i++
				) {
					range.Add( new mysIntegral( i ) );
				}

				return range;
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "range", functionGroup, global );
		}
	}
}
