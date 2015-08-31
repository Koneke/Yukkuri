using System;
using System.Collections.Generic;
using System.Linq;

namespace mysharp.Builtins
{
	public static class lambdaBuiltin  {
		static void Argumentcheck(
			mysSymbol symbol,
			mysList types,
			mysList symbols,
			mysList body
		) {
			if (
				symbol == null || types == null ||
				symbols == null || body == null
			) {
				throw new ArgumentException();
			}

			if (
				types.InternalValues.Count() !=
				symbols.InternalValues.Count()
			) {
				throw new ArgumentException();
			}

			if (
				types.InternalValues.Count <= 0 ||
				types.InternalValues
					.Any(t => (t as mysToken).Type != mysTypes.mysType)
			) {
				throw new ArgumentException();
			}

			if (
				symbols.InternalValues.Count <= 0 ||
				symbols.InternalValues
					.Any(t => (t as mysToken).Type != mysTypes.Symbol)
			) {
				throw new ArgumentException();
			}
		}

		public static mysToken Evaluate(
			mysSymbol symbol,
			mysList types,
			mysList symbols,
			mysList body,
			Stack<mysSymbolSpace> sss
		) {
			mysSymbolSpace ss = sss.Peek();

			Argumentcheck( symbol, types, symbols, body );

			// define function variant
			mysFunction f = new mysFunction();
			// these two should probably be joined at some point
			foreach ( mysToken t in types.InternalValues ) {
				f.Signature.Add( ( t as mysTypeToken ).TypeValue );
			}
			foreach ( mysToken t in symbols.InternalValues ) {
				f.Symbols.Add( t as mysSymbol  );
			}

			f.Function = body;
			// end define function variant

			mysFunctionGroup fg = null;

			// if symbol defined and of wrong type, undef it
			if (
				ss.Defined( symbol ) &&
				ss.GetValue( symbol ).Type != mysTypes.FunctionGroup
			) {
				// we could just overwrite it with define,
				// but I'd rather be entirely sure that we delete
				// the old value beforehand.
				ss.Undefine( symbol );
			}

			// if we're defined at this point, we know it's a function group
			if  ( ss.Defined( symbol ) ) {
				fg = ss.GetValue( symbol ) as mysFunctionGroup;
			} else {
				// create 
				fg = new mysFunctionGroup();
				ss.Define( symbol, fg );
			}

			fg.Variants.Add( f );

			// since we return our function group, unless quoted
			// we'll automatically evaluate it.
			// this is probably a good reason for allowing null returns.
			return fg.Quote();
		}

		public static void Setup( mysSymbolSpace global )
		{
			mysFunctionGroup lambda = new mysFunctionGroup();
			mysBuiltin lambdaVariant = new mysBuiltin();

			lambdaVariant = new mysBuiltin();
			lambdaVariant.Signature.Add( mysTypes.Symbol );
			lambdaVariant.Signature.Add( mysTypes.List );
			lambdaVariant.Signature.Add( mysTypes.List );
			lambdaVariant.Signature.Add( mysTypes.List );

			lambdaVariant.Function = (args, sss) => {
				mysSymbolSpace ss = sss.Peek();

				mysSymbol symbol = args[ 0 ] as mysSymbol;
				mysList types = args[ 1 ] as mysList;
				mysList symbols = args[ 2 ] as mysList;
				mysList body = args[ 3 ] as mysList;

				return Evaluate( symbol, types, symbols, body, sss );
			};

			lambda.Variants.Add( lambdaVariant );

			global.Define(
				global.Create( "=>" ),
				lambda
			);
		}
	}

	public static class AdditionBuiltin {
		static mysFunctionGroup functionGroup;

		static void setupIntIntVariant() {
			mysBuiltin variant = new mysBuiltin();

			variant.Signature.Add( mysTypes.Integral );
			variant.Signature.Add( mysTypes.Integral );

			variant.Function =
				new Func<List<mysToken>, Stack<mysSymbolSpace>, mysToken>(
				(args, sss) =>
					new mysIntegral(
						(args[ 0 ] as mysIntegral).Value +
						(args[ 1 ] as mysIntegral).Value
					)
			);

			functionGroup.Variants.Add( variant );
		}

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			setupIntIntVariant();

			global.Define(
				global.Create( "+" ),
				functionGroup
			);
		}
	}

	public static class SubtractionBuiltin {
		static mysFunctionGroup functionGroup;

		static void setupIntIntVariant() {
			mysBuiltin variant = new mysBuiltin();

			variant.Signature.Add( mysTypes.Integral );
			variant.Signature.Add( mysTypes.Integral );

			variant.Function =
				new Func<List<mysToken>, Stack<mysSymbolSpace>, mysToken>(
				(args, sss) =>
					new mysIntegral(
						(args[ 0 ] as mysIntegral).Value -
						(args[ 1 ] as mysIntegral).Value
					)
			);

			functionGroup.Variants.Add( variant );
		}

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			setupIntIntVariant();

			global.Define(
				global.Create( "-" ),
				functionGroup
			);
		}
	}

	public static class MultiplicationBuiltin {
		static mysFunctionGroup functionGroup;

		static void setupIntIntVariant() {
			mysBuiltin variant = new mysBuiltin();

			variant.Signature.Add( mysTypes.Integral );
			variant.Signature.Add( mysTypes.Integral );

			variant.Function =
				new Func<List<mysToken>, Stack<mysSymbolSpace>, mysToken>(
				(args, sss) =>
					new mysIntegral(
						(args[ 0 ] as mysIntegral).Value *
						(args[ 1 ] as mysIntegral).Value
					)
			);

			functionGroup.Variants.Add( variant );
		}

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			setupIntIntVariant();

			global.Define(
				global.Create( "*" ),
				functionGroup
			);
		}
	}

	public static class DivisionBuiltin {
		static mysFunctionGroup functionGroup;

		static void setupIntIntVariant() {
			mysBuiltin variant = new mysBuiltin();

			variant.Signature.Add( mysTypes.Integral );
			variant.Signature.Add( mysTypes.Integral );

			variant.Function =
				new Func<List<mysToken>, Stack<mysSymbolSpace>, mysToken>(
				(args, sss) =>
					new mysIntegral(
						(args[ 0 ] as mysIntegral).Value /
						(args[ 1 ] as mysIntegral).Value
					)
			);

			functionGroup.Variants.Add( variant );
		}

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			setupIntIntVariant();

			global.Define(
				global.Create( "/" ),
				functionGroup
			);
		}
	}
}

namespace mysharp
{
	public static class mysBuiltins {
		public static void Setup(
			mysSymbolSpace global
		) {
			Builtins.AdditionBuiltin.Setup( global );
			Builtins.SubtractionBuiltin.Setup( global );
			Builtins.MultiplicationBuiltin.Setup( global );
			Builtins.DivisionBuiltin.Setup( global );

			Builtins.lambdaBuiltin.Setup( global );
		}
	}

	public class mysBuiltin : mysFunction {
		public new Func<
			List<mysToken>,
			Stack<mysSymbolSpace>,
			mysToken
		> Function;

		// not sure we need to override? but I'm not chancing
		public override mysToken Call(
			Stack<mysSymbolSpace> spaceStack,
			List<mysToken> arguments
		) {
			return Function( arguments, spaceStack );
		}
	}
}
