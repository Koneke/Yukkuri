using System;

namespace mysharp.Builtins.Comparison
{
	public static class Equals
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( mysTypes.ANY );
			f.Signature.Add( mysTypes.ANY );

			f.Function = (args, state, sss) => {
				return new mysBoolean(
					args[ 0 ].InternalValue.Equals( args[ 1 ].InternalValue )
				);
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

			f.Signature.Add( mysTypes.NUMBER );
			f.Signature.Add( mysTypes.NUMBER );

			f.Function = (args, state, sss) => {
				Func<mysToken, mysFloating> toNumber = o =>
					o.Type == mysTypes.Integral
					? ( o as mysIntegral ).Promote()
					: o as mysFloating
				;

				mysFloating first = toNumber( args[ 0 ] );
				mysFloating second = toNumber( args[ 1 ] );

				return new mysBoolean(
					first.Value > second.Value
				);
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( ">", functionGroup, global );
		}
	}
}

