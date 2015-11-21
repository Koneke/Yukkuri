using System;
using System.Linq;
using System.Collections.Generic;

using mysharp.Parsing;

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
				ss.GetValue( symbol ).Type != typeof(mysFunctionGroup)
			) {
				// we could just overwrite it with define,
				// but I'd rather be entirely sure that we delete
				// the old value beforehand.
				ss.Undefine( symbol );
			}

			// if we're defined at this point, we know it's a function group
			if  ( ss.Defined( symbol ) ) {
				fg = ss.GetValue( symbol ).Value as mysFunctionGroup;
			} else {
				// create 
				fg = new mysFunctionGroup();

				ss.Define( symbol, new mysToken( fg ) );
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

			if ( value.Type == typeof(mysFunction) ) {
				defineFunction(
					symbol,
					value.Value as mysFunction,
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
				mysSymbol assignsymbol = args[ 0 ].Value as mysSymbol;
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
			List<mysToken> sig,
			List<mysToken> body
		) {
			if ( sig.Count %2 != 0 ) {
				throw new ArgumentException();
			}

			for ( int i = 0; i < sig.Count; i++ ) {
				if ( sig[ i ].Type !=
					( i % 2 == 0
						? typeof(mysSymbol)
						: typeof(Type) )
				) {
					throw new ArgumentException();
				}
			}
		}

		public static mysToken Evaluate(
			List<mysToken> sig,
			List<mysToken> body,
			Stack<mysSymbolSpace> sss
		) {
			mysSymbolSpace ss = sss.Peek();

			Argumentcheck( sig, body );

			mysFunction f = new mysFunction();

			// TODO: these two should probably be joined at some point
			for ( int i = 0; i < sig.Count; i++ ) {
				if ( sig[ i ].Type == typeof(mysSymbol) ) {
					f.Symbols.Add(
						sig[ i ].Value as mysSymbol
					);
				} else {
					Type t = (Type)sig[ i ].Value;
					f.Signature.Add( t );
				}
			}

			f.Function = body;

			return new mysToken( f );
		}

		public static void Setup( mysSymbolSpace global )
		{
			mysFunctionGroup lambda = new mysFunctionGroup();
			mysBuiltin lambdaVariant = new mysBuiltin();

			lambdaVariant = new mysBuiltin();
			lambdaVariant.Signature.Add( typeof(List<mysToken>) );
			lambdaVariant.Signature.Add( typeof(List<mysToken>) );

			lambdaVariant.Function = (args, state, sss) => {
				mysSymbolSpace ss = sss.Peek();

				List<mysToken> sig = (List<mysToken>)args[ 0 ].Value;
				List<mysToken> body = (List<mysToken>)args[ 1 ].Value;

				return new List<mysToken>() {
					Evaluate(
						sig,
						body,
						sss
					)
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
			f.Signature.Add( typeof(List<mysToken>) );

			f.Function = (args, state, sss) => {
				mysSymbol symbol = args[ 0 ].Value as mysSymbol;
				string ssName = symbol.StringRepresentation.ToLower();
				List<mysToken> body = (List<mysToken>)args[ 1 ].Value;

				if ( !state.nameSpaces.ContainsKey( ssName ) ) {
					state.nameSpaces.Add(
						ssName,
						new mysSymbolSpace()
					);
				}

				sss.Push( state.nameSpaces[ ssName ] );

				EvaluationMachine em = new EvaluationMachine(
					body,
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
			f.Signature.Add( typeof(List<mysToken>) );
			f.Signature.Add( typeof(List<mysToken>) );

			f.Function = (args, state, sss) => {
				List<mysToken> spaceList = (List<mysToken>)args[ 0 ].Value;
				List<mysToken> body = (List<mysToken>)args[ 1 ].Value;

				if ( !spaceList.All(
					v => v.Type == typeof(mysSymbol)
				) ) {
					throw new FormatException();
				}

				foreach( mysToken t in spaceList ) {
					mysSymbol symbol = t.Value as mysSymbol;
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
					body,
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
			f.Signature.Add( typeof(List<mysToken>) );

			f.Function = (args, state, sss) => {
				List<mysToken> expression = (List<mysToken>)args[ 0 ].Value;

				EvaluationMachine em = new EvaluationMachine(
					expression,
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
				string path = (string)args[ 0 ].Value;

				string source = System.IO.File.ReadAllText(
					System.IO.Path.Combine(
						System.IO.Directory.GetCurrentDirectory(),
						path
					)
				);

				// a bit clutzy atm, but I guess it does the trick for
				// the time being
				source = source.Replace( "@this-file", path );

				source = source.Replace(
					"@this-folder",
					path.Substring( 0, path.LastIndexOf( '/' ) )
				);

				List<mysToken> tokens = ParseMachine.Parse( state, source );

				state.Evaluate( tokens );

				return null;
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "load", functionGroup, global );
		}
	}
}
