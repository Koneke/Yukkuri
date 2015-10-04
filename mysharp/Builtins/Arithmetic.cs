using System.Collections.Generic;

namespace mysharp.Builtins.Arithmetic
{
	// might want to move this stuff to its own project?
	// and ref to that?
	// makes sense to keep the sort of "standard library" separate.

	public static class Addition {
		static void setupIntIntVariant( mysSymbolSpace global ) {
			mysBuiltin variant = new mysBuiltin();

			variant.Signature.Add( typeof(int) );
			variant.Signature.Add( typeof(int) );

			variant.Function = (args, state, sss) =>
				new List<mysToken>() {
					new mysToken(
						(int)args[ 0 ].InternalValue +
						(int)args[ 1 ].InternalValue
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

			variant.Signature.Add( typeof(int) );
			variant.Signature.Add( typeof(int) );

			variant.Function = (args, state, sss) =>
				new List<mysToken>() {
					new mysToken(
						(int)args[ 0 ].InternalValue -
						(int)args[ 1 ].InternalValue
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

			variant.Signature.Add( typeof(int) );
			variant.Signature.Add( typeof(int) );

			variant.Function = (args, state, sss) =>
				new List<mysToken>() {
					new mysToken(
						(int)args[ 0 ].InternalValue *
						(int)args[ 1 ].InternalValue
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

			variant.Signature.Add( typeof(int) );
			variant.Signature.Add( typeof(int) );

			variant.Function = (args, state, sss) =>
				new List<mysToken>() {
					new mysToken(
						(int)args[ 0 ].InternalValue /
						(int)args[ 1 ].InternalValue
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
