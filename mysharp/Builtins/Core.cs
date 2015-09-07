using System;
using System.Linq;
using System.Collections.Generic;

namespace mysharp.Builtins.Core {
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
			Stack<mysSymbolSpace> spaceStack
		) {
			// NOTICE THIS
			// since each function has it's own internal space
			// before grabbing our reference to the space in which
			// we want to define our symbol, we need to pop the
			// internal off, or we're going to be defining the symbol
			// in our internal space, i.e. it will scope out as soon as
			// we're done. So we pop the internal off, grab our reference
			// to the space outside of that, then push the internal back on.
			mysSymbolSpace top = spaceStack.Pop();
			mysSymbolSpace ss = spaceStack.Peek();

			switch ( value.Type ) {
				case mysTypes.Function:
					defineFunction(
						symbol,
						value as mysFunction,
						spaceStack.Peek()
					);
					break;

				default:
					mysSymbolSpace space = symbol.DefinedIn( spaceStack );
					if ( space != null ) {
						space.Define( symbol, value );
					} else {
						//spaceStack.Peek().Define( symbol, value );
						ss.Define( symbol, value );
					}

					//spaceStack.Peek().Define( symbol, value );
					break;
			}

			spaceStack.Push( top );
			return null;
			//return value;
		}

		public static void Setup( mysSymbolSpace global )
		{
			mysFunctionGroup assign = new mysFunctionGroup();
			mysBuiltin assignVariant = new mysBuiltin();

			assignVariant = new mysBuiltin();
			assignVariant.Signature.Add( mysTypes.Symbol );
			assignVariant.Signature.Add( mysTypes.ANY );

			assignVariant.Function = (args, state, sss) => {
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

			lambdaVariant.Function = (args, state, sss) => {
				mysSymbolSpace ss = sss.Peek();

				mysList sig = args[ 0 ] as mysList;
				mysList body = args[ 1 ] as mysList;

				return Evaluate( sig, body, sss );
			};

			lambda.Variants.Add( lambdaVariant );

			mysBuiltin.DefineInGlobal( "=>", lambda, global );
		}
	}
}

