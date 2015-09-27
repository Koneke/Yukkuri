using System.Collections.Generic;

namespace mysharp.Builtins.Arithmetic
{
	// might want to move this stuff to its own project?
	// and ref to that?
	// makes sense to keep the sort of "standard library" separate.

	public static class Addition {
		static mysFunctionGroup functionGroup;

		static void setupIntIntVariant( mysSymbolSpace global ) {
			mysBuiltin variant = new mysBuiltin();

			variant.ReturnType = mysTypes.Integral;

			variant.Signature.Add( mysTypes.Integral );
			variant.Signature.Add( mysTypes.Integral );

			variant.Function = (args, state, sss) =>
				new List<mysToken>() {
					new mysIntegral(
						(args[ 0 ] as mysIntegral).Value +
						(args[ 1 ] as mysIntegral).Value
					)
				};

			mysBuiltin.AddVariant( "+", variant, global );
		}

		public static void Setup( mysSymbolSpace global ) {
			setupIntIntVariant( global );
		}
	}

	public static class Subtraction {
		static mysFunctionGroup functionGroup;

		static void setupIntIntVariant() {
			mysBuiltin variant = new mysBuiltin();

			variant.ReturnType = mysTypes.Integral;

			variant.Signature.Add( mysTypes.Integral );
			variant.Signature.Add( mysTypes.Integral );

			variant.Function = (args, state, sss) =>
				new List<mysToken>() {
					new mysIntegral(
						(args[ 0 ] as mysIntegral).Value -
						(args[ 1 ] as mysIntegral).Value
					)
				};

			functionGroup.Variants.Add( variant );
		}

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			setupIntIntVariant();

			mysBuiltin.DefineInGlobal( "-", functionGroup, global );
		}
	}

	public static class Multiplication {
		static mysFunctionGroup functionGroup;

		static void setupIntIntVariant() {
			mysBuiltin variant = new mysBuiltin();

			variant.ReturnType = mysTypes.Integral;

			variant.Signature.Add( mysTypes.Integral );
			variant.Signature.Add( mysTypes.Integral );

			variant.Function = (args, state, sss) =>
				new List<mysToken>() {
					new mysIntegral(
						(args[ 0 ] as mysIntegral).Value *
						(args[ 1 ] as mysIntegral).Value
					)
				};

			functionGroup.Variants.Add( variant );
		}

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			setupIntIntVariant();

			mysBuiltin.DefineInGlobal( "*", functionGroup, global );
		}
	}

	public static class Division {
		static mysFunctionGroup functionGroup;

		static void setupIntIntVariant() {
			mysBuiltin variant = new mysBuiltin();

			variant.ReturnType = mysTypes.Integral;

			variant.Signature.Add( mysTypes.Integral );
			variant.Signature.Add( mysTypes.Integral );

			variant.Function = (args, state, sss) =>
				new List<mysToken>() {
					new mysIntegral(
						(args[ 0 ] as mysIntegral).Value /
						(args[ 1 ] as mysIntegral).Value
					)
				};

			functionGroup.Variants.Add( variant );
		}

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			setupIntIntVariant();

			mysBuiltin.DefineInGlobal( "/", functionGroup, global );
		}
	}
}
