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
			//repl.TestFunction();
		}
	}
	public class mysREPL
	{
		// just for ease of access/reading
		public mysSymbolSpace Global;

		public Dictionary<string, mysSymbolSpace> nameSpaces;

		// not sure if we actually need this in the repl...?
		// might be enough to just pass in global
		public Stack<mysSymbolSpace> spaceStack;

		public mysREPL() {
			nameSpaces = new Dictionary<string, mysSymbolSpace>();

			Global = new mysSymbolSpace();
			nameSpaces.Add( "global" , Global );

			spaceStack = new Stack<mysSymbolSpace>();
			spaceStack.Push( Global );

			mysBuiltins.Setup( Global );

			List<mysToken> parsed;
			mysParser parser = new mysParser();

			bool quit = false;
			while ( !quit ) {
				Console.Write( " > " );
				string input = Console.ReadLine();
				if ( input == "(quit)" ) {
					quit = true;
				} else {
					parsed = parser.Parse( input );

					EvaluationMachine em = new EvaluationMachine();
					List<mysToken> output = em.Evaluate( parsed, spaceStack );

					Console.WriteLine( string.Join( ", ", output ) );

					Console.WriteLine( "Ok.\n" );
				}
			}
		}

		/*
		public List<mysToken> Evaluate(
			List<mysToken> tokens,
			Stack<mysSymbolSpace> spaceStack
		) {
			// while any list
			// eval list
			while ( true ) {
				mysList list = tokens.First( t =>
					t.Type == mysTypes.List &&
					!t.Quoted
				) as mysList;

				int index = tokens.IndexOf( list );
				tokens.Remove( list );

				tokens.InsertRange(
					index,
					Evaluate( list.InternalValues )
				);
			}

			for ( int i = 0; i < tokens.Count(); i++ ) {
				if ( tokens[ i ].Type == mysTypes.Symbol ) {
					if ( !tokens[ i ].Quoted ) {
						tokens[ i ] = EvaluateSymbol(
							tokens[ i ] as mysSymbol,
							spaceStack
						);
					}
				}

				if ( tokens[ i ].Type == mysTypes.FunctionGroup ) {
					mysFunctionGroup fg = tokens[ i ] as mysFunctionGroup;
				}

				switch ( tokens[ i ].Type ) {
					case mysTypes.Function:
						break;
					case mysTypes.List:
						break;
					// only quoted symbols end up here, so we ok
					case mysTypes.Symbol:
					case mysTypes.Integral:
					case mysTypes.Floating:
					case mysTypes.mysType:
						break;
					default:
						throw new ArgumentException();
				}
			}

			// function arg pairing

			// exec

			// return

			throw new NotImplementedException();
		}

		public static mysTypes EvaluateSymbolType(
			mysSymbol symbol,
			Stack<mysSymbolSpace> spaceStack
		) {
			Stack<mysSymbolSpace> evaluationStack = spaceStack.Clone();

			mysToken temp = new mysSymbol( symbol.ToString() );

			while ( temp.Type == mysTypes.Symbol ) {
				temp = EvaluateSymbol( symbol, evaluationStack );
			}

			return temp.Type;
		} 

		public static mysToken EvaluateSymbol(
			mysSymbol symbol,
			Stack<mysSymbolSpace> spaceStack
		) {
			Stack<mysSymbolSpace> evaluationStack = spaceStack.Clone();

			while ( evaluationStack.Count > 0 ) {
				mysSymbolSpace space = evaluationStack.Pop();

				if ( space.Defined( symbol ) ) {
					return space.GetValue( symbol );
				}
			}

			throw new ArgumentException( "Symbol isn't defined." );
		}
		*/
		/*
		public void TestFunction() {
			mysParser parser = new mysParser();

			mysToken result;
			mysList parsed;

			parsed = parser.Parse(
				"+ (+ 3 1) (+ 2 4)"
			);
			result = parsed.Evaluate( spaceStack );

			parsed = parser.Parse(
				"+ 1 3"
			);
			//result = parsed.Evaluate( spaceStack );

			parsed = parser.Parse(
				"+ (- 3 1) 2"
			);
			result = parsed.Evaluate( spaceStack );

			parsed = parser.Parse(
				"+ - 3 1 2"
			);
			result = parsed.Evaluate( spaceStack );

			parsed = parser.Parse(
				//"(+ (- 3 1) 2)"
				"(=> 'some-func '(:int) '(x) '(+ 3 x))"
				//"(+ 1 2)"
			);
			result = parsed.Evaluate( spaceStack );

			parsed = parser.Parse(
				"(some-func 2)"
			);
			result = parsed.Evaluate( spaceStack );

			parsed = parser.Parse(
				"some-func some-func 2"
			);
			result = parsed.Evaluate( spaceStack );

			var a = 0;
		}
		*/
	}
}
