using System.Collections.Generic;

namespace mysharp.Builtins.Comparison
{
	public static class Equals
	{
		static mysFunctionGroup functionGroup;

		static bool CompareNumbers( mysToken a, mysToken b ) {
			mysToken first = mysToken.PromoteToFloat( a );
			mysToken second = mysToken.PromoteToFloat( b );

			return
				(float)first.InternalValue ==
				(float)second.InternalValue;
		}

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( typeof(ANY) );
			f.Signature.Add( typeof(ANY) );

			f.Function = (args, state, sss) => {
				if (
					mysToken.AssignableFrom(
						typeof(NUMBER),
						args[ 0 ].RealType
					) &&
					mysToken.AssignableFrom(
						typeof(NUMBER),
						args[ 1 ].RealType
					)
				) {
					return new List<mysToken>() {
						new mysToken(
							CompareNumbers( args[ 0 ], args[ 1 ] )
						)
					};
				}

				return new List<mysToken>() {
					new mysToken(
						args[ 0 ].InternalValue
						.Equals( args[ 1 ].InternalValue )
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
						(float)first.InternalValue >
						(float)second.InternalValue
					)
				};
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( ">", functionGroup, global );
		}
	}
}

