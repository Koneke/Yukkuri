using System;
using System.Linq;
using System.Collections.Generic;

namespace mysharp.Builtins
{
	// might want to move this stuff to its own project?
	// and ref to that?
	// makes sense to keep the sort of "standard library" separate.

	public static class Addition {
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

			mysBuiltin.DefineInGlobal( "+", functionGroup, global );
		}
	}

	public static class Subtraction {
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

			mysBuiltin.DefineInGlobal( "/", functionGroup, global );
		}
	}

	public static class Assign {
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
			
			mysBuiltin.DefineInGlobal( "def", assign, global );
		}
	}

	public static class Lambda  {
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
							.Value
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

			mysBuiltin.DefineInGlobal( "=>", lambda, global );
		}
	}

	public static class Car {
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			//f.returnType

			f.Signature.Add( mysTypes.List );

			f.Function = (args, sss) =>
				( args[ 0 ] as mysList ).InternalValues
					.FirstOrDefault()
			;

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "car", functionGroup, global );
		}
	}

	public static class Cdr {
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( mysTypes.List );

			f.Function = (args, sss) =>
				new mysList(
					( args[ 0 ] as mysList ).InternalValues
						.Skip( 1 )
						.ToList()
				).Quote( args[ 0 ].Quoted )
			;

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "cdr", functionGroup, global );
		}
	}

	public static class NewClrObject
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( mysTypes.String );

			f.Function = (args, sss) =>
				new mysList()
			;

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "#new", functionGroup, global );
		}
	}
}

namespace mysharp
{
	public static class mysBuiltins {
		public static void Setup(
			mysSymbolSpace global
		) {
			Builtins.Addition.Setup( global );
			Builtins.Subtraction.Setup( global );
			Builtins.Multiplication.Setup( global );
			Builtins.Division.Setup( global );

			Builtins.Lambda.Setup( global );
			Builtins.Assign.Setup( global );

			Builtins.Car.Setup( global );
			Builtins.Cdr.Setup( global );
		}
	}

	public class mysBuiltin : mysFunction {
		public static void DefineInGlobal(
			string name,
			mysFunctionGroup fg,
			mysSymbolSpace global
		) {
			mysSymbol symbol = global.Create( name );
			symbol.Type = mysTypes.FunctionGroup;
			fg.Type = mysTypes.FunctionGroup;
			
			global.Define( symbol, fg );
		}
		
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
				? ( t as mysSymbol ).Value( spaceStack )
				: t
			).ToList();

			return Function( arguments, spaceStack );
		}
	}
}
