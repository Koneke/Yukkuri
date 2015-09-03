using System;
using System.Collections.Generic;
using System.Linq;

namespace mysharp.Builtins
{
	public static class assignBuiltin {
		static void defineFunction(
			mysSymbol symbol,
			mysFunction f,
			mysSymbolSpace ss
		) {
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
				fg.Type = mysTypes.FunctionGroup;

				ss.Define( symbol, fg );
				symbol.Type = mysTypes.FunctionGroup;
			}

			mysFunction collision = fg.Variants.FirstOrDefault(
				v => v.Signature
					.Zip( f.Signature, (a, b) => a == b )
					.Count() == v.SignatureLength
			);

			if ( collision != null ) {
				// overwrite a conflicting sig! should probably
				// notify the user about this when it happens.
				fg.Variants.Remove( collision );
			}

			fg.Variants.Add( f );
		}

		public static mysToken Evaluate(
			mysSymbol symbol,
			mysToken value,
			Stack<mysSymbolSpace> stackSpace
		) {

			switch ( value.Type ) {
				case mysTypes.Function:
					defineFunction(
						symbol,
						value as mysFunction,
						stackSpace.Peek()
					);
					break;

				default:
					stackSpace.Peek().Define( symbol, value );
					break;
			}

			return null;
		}

		public static void Setup( mysSymbolSpace global )
		{
			mysFunctionGroup assign = new mysFunctionGroup();
			mysBuiltin assignVariant = new mysBuiltin();

			assignVariant = new mysBuiltin();
			assignVariant.Signature.Add( mysTypes.Symbol );
			assignVariant.Signature.Add( mysTypes.ANY );

			assignVariant.Function = (args, sss) => {
				mysSymbolSpace ss = sss.Peek();

				mysSymbol assignsymbol = args[ 0 ] as mysSymbol;
				mysToken value = args[ 1 ];

				return Evaluate( assignsymbol, value, sss );
			};

			assign.Variants.Add( assignVariant );
			
			mysSymbol symbol = global.Create( "def" );
			assign.Type = mysTypes.FunctionGroup;

			global.Define(
				symbol,
				assign
			);
		}
	}

	public static class lambdaBuiltin  {
		static void Argumentcheck(
			mysList sig,
			mysList body
		) {
			if ( sig.InternalValues.Count %2 != 0 ) {
				throw new ArgumentException();
			}

			for ( int i = 0; i < sig.InternalValues.Count; i++ ) {
				if ( sig.InternalValues[ i ].Type !=
					( i % 2 == 0
						? mysTypes.Symbol
						: mysTypes.mysType )
				) {
					throw new ArgumentException();
				}
			}
		}

		public static mysToken Evaluate(
			mysList sig,
			mysList body,
			Stack<mysSymbolSpace> sss
		) {
			mysSymbolSpace ss = sss.Peek();

			Argumentcheck( sig, body );

			// define function variant
			mysFunction f = new mysFunction();

			// these two should probably be joined at some point
			for ( int i = 0; i < sig.InternalValues.Count; i++ ) {
				if ( sig.InternalValues[ i ].Type == mysTypes.Symbol ) {
					f.Symbols.Add(
						sig.InternalValues[ i ] as mysSymbol
					);
				} else {
					f.Signature.Add(
						( sig.InternalValues[ i ] as mysTypeToken )
							.TypeValue
					);
				}
			}

			f.Function = body;
			// end define function variant

			return f;
		}

		public static void Setup( mysSymbolSpace global )
		{
			mysFunctionGroup lambda = new mysFunctionGroup();
			mysBuiltin lambdaVariant = new mysBuiltin();

			lambdaVariant = new mysBuiltin();
			lambdaVariant.Signature.Add( mysTypes.List );
			lambdaVariant.Signature.Add( mysTypes.List );

			lambdaVariant.Function = (args, sss) => {
				mysSymbolSpace ss = sss.Peek();

				mysList sig = args[ 0 ] as mysList;
				mysList body = args[ 1 ] as mysList;

				return Evaluate( sig, body, sss );
			};

			lambda.Variants.Add( lambdaVariant );
			
			mysSymbol symbol = global.Create( "=>" );
			symbol.Type = mysTypes.FunctionGroup;
			lambda.Type = mysTypes.FunctionGroup;

			global.Define(
				symbol,
				lambda
			);
		}
	}

	public static class AdditionBuiltin {
		static mysFunctionGroup functionGroup;

		static void setupIntIntVariant() {
			mysBuiltin variant = new mysBuiltin();

			variant.ReturnType = mysTypes.Integral;

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

			mysSymbol symbol = global.Create( "+" );
			symbol.Type = mysTypes.FunctionGroup;
			functionGroup.Type = mysTypes.FunctionGroup;

			global.Define(
				symbol,
				functionGroup
			);
		}
	}

	public static class SubtractionBuiltin {
		static mysFunctionGroup functionGroup;

		static void setupIntIntVariant() {
			mysBuiltin variant = new mysBuiltin();

			variant.ReturnType = mysTypes.Integral;

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

			mysSymbol symbol = global.Create( "-" );
			symbol.Type = mysTypes.FunctionGroup;
			functionGroup.Type = mysTypes.FunctionGroup;

			global.Define(
				symbol,
				functionGroup
			);
		}
	}

	public static class MultiplicationBuiltin {
		static mysFunctionGroup functionGroup;

		static void setupIntIntVariant() {
			mysBuiltin variant = new mysBuiltin();

			variant.ReturnType = mysTypes.Integral;

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

			mysSymbol symbol = global.Create( "*" );
			symbol.Type = mysTypes.FunctionGroup;
			functionGroup.Type = mysTypes.FunctionGroup;
			
			global.Define(
				symbol,
				functionGroup
			);
		}
	}

	public static class DivisionBuiltin {
		static mysFunctionGroup functionGroup;

		static void setupIntIntVariant() {
			mysBuiltin variant = new mysBuiltin();

			variant.ReturnType = mysTypes.Integral;

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

			mysSymbol symbol = global.Create( "/" );
			symbol.Type = mysTypes.FunctionGroup;
			functionGroup.Type = mysTypes.FunctionGroup;
			
			global.Define(
				symbol,
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
			Builtins.assignBuiltin.Setup( global );
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
			arguments = arguments.Select( t =>
				t.Type == mysTypes.Symbol && !t.Quoted
				? EvaluationMachine.EvaluateSymbol(
					t as mysSymbol,
					spaceStack)
				: t
			).ToList();

			return Function( arguments, spaceStack );
		}
	}
}
