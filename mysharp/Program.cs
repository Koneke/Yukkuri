using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace mysharp
{
	class NoSuchSignatureException : Exception { }

	public static class Extensions
	{
		public static void DoPaired<T, U>(
			this IEnumerable<T> t,
			IEnumerable<U> other,
			Action<T, U> action
		) {
			for ( int i = 0; i < Math.Min( t.Count(), other.Count() ); i++ ) {
				action( t.ElementAt( i ), other.ElementAt( i ) );
			}
		}

		public static Stack<T> Clone<T>(this Stack<T> stack) {
			Contract.Requires( stack != null );
			return new Stack<T>( new Stack<T>( stack ) );
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			mysREPL repl = new mysREPL();
			repl.TestFunction();
		}
	}

	public class mysParser
	{
		// parses SIMPLE VALUES, NOT LISTS
		public mysToken ParseLex( string s ) {
			//bool quote = false;
			mysToken token = null;

			/*if ( s[ 0 ] == '\'' ) {
				quote = true;
				s = s.Substring( 1, s.Length - 1 );
			}*/

			if ( s[ 0 ] == ':' ) {
				switch ( s.Substring( 1, s.Length - 1 ) ) {
					case "int":
						token = new mysTypeToken( mysTypes.Integral );
						break;
					default:
						throw new ArgumentException();
				}
			} else if ( s.All( c => c >= '0' && c <= '9' ) ) {
				token = new mysIntegral( long.Parse( s ) );
			} else if ( s.All( c =>
				c == '.' || c == ',' ||
				(c >= '0' && c <= '9' ) )
			) {
				s = s.Replace( '.', ',' );
				token = new mysFloating( double.Parse( s ) );
			} else {
				token = new mysSymbol( s );
			}

			//if ( token == null )
				//throw new ArgumentException();

			/*if ( quote ) {
				token.Quote();
			}*/

			return token;
		}

		public class Lex {
			public bool Simple;
			public string Body;

			public Lex( string body, bool simple = false ) {
				Body = body;
				Simple = simple;
			}
		}

		public mysList Parse( string expression ) {
			List<string> split = expression
				.Replace( "(", " ( " )
				.Replace( ")", " ) " )
				.Replace( "'", " ' " )
				.Split(' ')
				.Where( sub => sub != " " && sub != "" )
				.ToList()
			;

			List<mysToken> tokens = new List<mysToken>();

			/*if ( split.First() == "(" && split.Last() == ")" ) {
				split.RemoveAt( split.Count - 1 );
				split.RemoveAt( 0 );
			}*/

			bool quote = false;

			for (
				int startToken = 0;
				startToken < split.Count;
				startToken++
			) {
				if ( split[ startToken ] == "(" ) {
					int depth = 0;

					for (
						int endToken = startToken + 1;
						endToken < split.Count;
						endToken++
					) {
						if ( split[ endToken ] == "(" ) {
							depth++;
						} else if ( split[ endToken ] == ")" ) {
							depth--;
							if ( depth == -1 ) {
								int count = endToken - startToken + 1;

								string body = 
									string.Join(
										" ",
										split
											.Skip( startToken + 1 )
											.Take( count - 2 )
									);

								tokens.Add(
									Parse( body )
										.Quote( quote )
								);

								quote = false;

								split.RemoveRange(
									startToken,
									endToken - startToken + 1
								);
								startToken--;
								break;
							}
						}
					}
				} else if ( split[ startToken ] == "'" ) {
					quote = true;

					split.RemoveAt( startToken );
					startToken--;
				} else {
					// simple value
					tokens.Add(
						ParseLex( split[ startToken ] )
							.Quote( quote )
					);

					quote = false;

					split.RemoveAt( startToken );
					startToken--;
					var a = 0; // just for breaking
				}
			}

			return new mysList( tokens );
		}
	}

	public class mysREPL
	{
		public mysSymbolSpace Global;
		public Dictionary<string, mysSymbolSpace> nameSpaces;
		public Stack<mysSymbolSpace> spaceStack;

		public mysREPL() {
			nameSpaces = new Dictionary<string, mysSymbolSpace>();

			Global = new mysSymbolSpace();
			nameSpaces.Add( "global" , Global );

			spaceStack = new Stack<mysSymbolSpace>();
			spaceStack.Push( Global );

			mysBuiltins.Setup( Global );

			mysParser parser = new mysParser();

			mysToken result;
			mysList parsed;
			//result = Parse( "100.3" );
			//result = Parse( "7,8" );
			//result = parser.Parse( "foo (some list)" );
			//result = parser.Parse( "(+ 1 2)" );
			//result = parser.Parse( "=> some-func '(:int) '(x) '(+ 3 x)" );
			//result = parser.Parse( "(+ (- 3 1) 2)" );

			//parsed = parser.Parse( "(* (+ (- 3 1) 2) 4)" );
			//result = parsed.Evaluate( spaceStack );

			//parsed = parser.Parse( "(+ (+ 1 2) (+ 3 4))" );
			//result = parsed.Evaluate( spaceStack );

			//parsed = parser.Parse( "=> 'some-func '(:int) '(x) '(+ 3 x)" );
			//result = parsed.Evaluate( spaceStack );

			//parsed = parser.Parse( "some-func 2" );
			//result = parsed.Evaluate( spaceStack );
			
			parsed = parser.Parse(
				"(some-func 2)" +
				"(=> 'some-func '(:int) '(x) '(+ 3 x))"
				//+ "(some-func 2)"
			);
			result = parsed.Evaluate( spaceStack );

			var a = 0;
		}

		public void TestFunction() {
			// lh: (+ x y)
			//     (=> some-func '(int) '(x) '(+ x 3))

			List<mysToken> testExpression = new List<mysToken>();

			mysFunctionGroup fg = new mysFunctionGroup();
			mysFunction f = new mysFunction();

			f.Symbols.Add( new mysSymbol( "x" ) );
			f.Signature.Add( mysTypes.Integral );
			f.Symbols.Add( new mysSymbol( "y" ) );
			f.Signature.Add( mysTypes.Integral );

			f.Function.InternalValues.Add(
				mysSymbolSpace.GetAndEvaluateSymbol( "+", spaceStack )
			);

			f.Function.InternalValues.Add( new mysSymbol( "x" ) );
			f.Function.InternalValues.Add( new mysSymbol( "y" ) );

			fg.Variants.Add( f );

			testExpression.Add( fg );
			testExpression.Add( new mysIntegral( 2 ) );
			testExpression.Add( new mysIntegral( 3 ) );

			mysList expression = new mysList( testExpression );
			mysToken result = expression.Evaluate( spaceStack );

			var a = 0;

			// lambda test

			testExpression = new List<mysToken>();

			testExpression.Add(
				mysSymbolSpace.GetAndEvaluateSymbol( "=>", spaceStack )
			);

			testExpression.Add(
				new mysSymbol( "some-func" ).Quote()
			);

			testExpression.Add( new mysList( new List<mysToken> {
				new mysTypeToken( mysTypes.Integral )
			} ).Quote() );

			testExpression.Add( new mysList( new List<mysToken> {
				new mysSymbol( "x" )
			} ).Quote() );

			fg = mysSymbolSpace.GetAndEvaluateSymbol( "+", spaceStack )
				as mysFunctionGroup;

			testExpression.Add( new mysList( new List<mysToken> {
				fg,
				new mysSymbol( "x" ),
				new mysIntegral( 3 )
			} ).Quote() );

			expression = new mysList( testExpression );
			result = expression.Evaluate( spaceStack );

			a = 0;

			// lambda result test

			testExpression = new List<mysToken>();

			testExpression.Add(
				mysSymbolSpace.GetAndEvaluateSymbol( "some-func", spaceStack )
			);
			testExpression.Add( new mysIntegral( 2 ) );

			expression = new mysList( testExpression );
			result = expression.Evaluate( spaceStack );

			a = 0;
		}
	}
}
