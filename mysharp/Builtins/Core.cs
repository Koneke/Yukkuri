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
				ss.GetValue( symbol ).RealType != typeof(mysFunctionGroup)
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
				//symbol.Type = mysTypes.FunctionGroup;
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

			if ( value.RealType == typeof(mysFunction) ) {
				defineFunction(
					symbol,
					value as mysFunction,
					spaceStack.Peek()
				);
			} else {
				mysSymbolSpace space = symbol.DefinedIn( spaceStack );
				if ( space != null ) {
					space.Define( symbol, value );
				} else {
					ss.Define( symbol, value );
				}
			}

			spaceStack.Push( top );
			return null;
		}

		public static void Setup( mysSymbolSpace global )
		{
			mysFunctionGroup assign = new mysFunctionGroup();
			mysBuiltin assignVariant = new mysBuiltin();

			assignVariant = new mysBuiltin();
			assignVariant.Signature.Add( typeof(mysSymbol) );
			assignVariant.Signature.Add( typeof(ANY) );

			assignVariant.Function = (args, state, sss) => {
				mysSymbol assignsymbol = args[ 0 ] as mysSymbol;
				mysToken value = args[ 1 ];

				return new List<mysToken>() {
					Evaluate( assignsymbol, value, sss )
				};
			};

			assign.Variants.Add( assignVariant );
			
			mysBuiltin.DefineInGlobal( "def", assign, global );
		}
	}

	public static class Lambda {
		static void Argumentcheck(
			mysList sig,
			mysList body
		) {
			if ( sig.InternalValues.Count %2 != 0 ) {
				throw new ArgumentException();
			}

			for ( int i = 0; i < sig.InternalValues.Count; i++ ) {
				if ( sig.InternalValues[ i ].RealType !=
					( i % 2 == 0
						? typeof(mysSymbol)
						: typeof(Type) )
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
				if ( sig.InternalValues[ i ].RealType == typeof(mysSymbol) ) {
					f.Symbols.Add(
						sig.InternalValues[ i ] as mysSymbol
					);
				} else {
					Type t = sig.InternalValues[ i ].RealType;
					f.Signature.Add( t );
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
			lambdaVariant.Signature.Add( typeof(mysList) );
			lambdaVariant.Signature.Add( typeof(mysList) );

			lambdaVariant.Function = (args, state, sss) => {
				mysSymbolSpace ss = sss.Peek();

				mysList sig = args[ 0 ] as mysList;
				mysList body = args[ 1 ] as mysList;

				return new List<mysToken>() {
					Evaluate( sig, body, sss )
				};
			};

			lambda.Variants.Add( lambdaVariant );

			mysBuiltin.DefineInGlobal( "=>", lambda, global );
		}
	}

	public static class InNamespace {
		public static mysFunctionGroup functionGroup;
		public static void Setup( mysSymbolSpace global )
		{
			functionGroup = new mysFunctionGroup();
			mysBuiltin f;

			// single value version

			f = new mysBuiltin();

			f = new mysBuiltin();
			f.Signature.Add( typeof(mysSymbol) );
			f.Signature.Add( typeof(mysList) );

			f.Function = (args, state, sss) => {
				mysSymbol symbol = args[ 0 ] as mysSymbol;
				string ssName = symbol.StringRepresentation.ToLower();
				mysList body = args[ 1 ] as mysList;

				if ( !state.nameSpaces.ContainsKey( ssName ) ) {
					state.nameSpaces.Add(
						ssName,
						new mysSymbolSpace()
					);
				}

				sss.Push( state.nameSpaces[ ssName ] );

				EvaluationMachine em = new EvaluationMachine(
					body.InternalValues,
					state,
					sss
				);

				// really needs to be fixed
				List<mysToken> result = em.Evaluate();

				sss.Pop();

				return result;
			};

			functionGroup.Variants.Add( f );

			// list version

			f = new mysBuiltin();

			f = new mysBuiltin();
			f.Signature.Add( typeof(mysList) );
			f.Signature.Add( typeof(mysList) );

			f.Function = (args, state, sss) => {
				mysList spaceList = args[ 0 ] as mysList;
				mysList body = args[ 1 ] as mysList;

				if ( !spaceList.InternalValues.All(
					v => v.RealType == typeof(mysSymbol)
				) ) {
					throw new FormatException();
				}

				foreach( mysSymbol symbol in spaceList.InternalValues ) {
					string ssName = symbol.StringRepresentation.ToLower();

					if ( !state.nameSpaces.ContainsKey( ssName ) ) {
						state.nameSpaces.Add(
							ssName,
							new mysSymbolSpace()
						);
					}

					sss.Push( state.nameSpaces[ ssName ] );
				}

				EvaluationMachine em = new EvaluationMachine(
					body.InternalValues,
					state,
					sss
				);

				// really needs to be fixed
				List<mysToken> result = em.Evaluate();

				sss.Pop();

				return result;
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "in", functionGroup, global );
		}
	}

	public static class Eval {
		public static mysFunctionGroup functionGroup;
		public static void Setup( mysSymbolSpace global )
		{
			functionGroup = new mysFunctionGroup();
			mysBuiltin f = new mysBuiltin();

			f = new mysBuiltin();
			f.Signature.Add( typeof(mysList) );

			f.Function = (args, state, sss) => {
				mysList expression = args[ 0 ] as mysList;

				EvaluationMachine em = new EvaluationMachine(
					expression.InternalValues,
					state,
					sss
				);

				return em.Evaluate();
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "eval", functionGroup, global );
		}
	}

	public static class ToString {
		public static mysFunctionGroup functionGroup;
		public static void Setup( mysSymbolSpace global )
		{
			functionGroup = new mysFunctionGroup();
			mysBuiltin f = new mysBuiltin();

			f = new mysBuiltin();
			f.Signature.Add( typeof(ANY) );

			f.Function = (args, state, sss) => {
				return new List<mysToken>() {
					new mysToken( args[ 0 ].ToString() )
				};
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "string", functionGroup, global );
		}
	}

	public static class Load {
		public static mysFunctionGroup functionGroup;
		public static void Setup( mysSymbolSpace global )
		{
			functionGroup = new mysFunctionGroup();
			mysBuiltin f = new mysBuiltin();

			f = new mysBuiltin();
			f.Signature.Add( typeof(string) );

			f.Function = (args, state, sss) => {
				mysParser parser = new mysParser();

				string path = (string)args[ 0 ].InternalValue;

				string source = System.IO.File.ReadAllText(
					System.IO.Path.Combine(
						System.IO.Directory.GetCurrentDirectory(),
						path
					)
				);

				List<mysToken> tokens = parser.Parse( state, source );

				state.Evaluate( tokens );

				return null;
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "load", functionGroup, global );
		}
	}
}
