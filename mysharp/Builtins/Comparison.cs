namespace mysharp.Builtins.Comparison
{
	public static class Equals
	{
		static mysFunctionGroup functionGroup;

		static bool CompareNumbers( mysToken a, mysToken b ) {
			mysFloating first = mysToken.PromoteNumber( a );
			mysFloating second = mysToken.PromoteNumber( b );

			return first.Value == second.Value;
		}

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( mysTypes.ANY );
			f.Signature.Add( mysTypes.ANY );

			f.Function = (args, state, sss) => {
				if (
					mysToken.AssignableFrom(
						mysTypes.NUMBER,
						args[ 0 ].Type
					) &&
					mysToken.AssignableFrom(
						mysTypes.NUMBER,
						args[ 1 ].Type
					)
				) {
					return new mysBoolean(
						CompareNumbers( args[ 0 ], args[ 1 ] )
					);
				}

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
				mysToken.PromoteNumber( args[ 0 ] );
				mysToken.PromoteNumber( args[ 1 ] );

				mysFloating first = mysToken.PromoteNumber( args[ 0 ] );
				mysFloating second = mysToken.PromoteNumber( args[ 1 ] );

				return new mysBoolean(
					first.Value > second.Value
				);
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( ">", functionGroup, global );
		}
	}
}

