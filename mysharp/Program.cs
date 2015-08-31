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

			mysList result;
			result = Parse( "100.3" );
			result = Parse( "7,8" );
			result = Parse( "=> some-func '(:int) '(x) '(+ 3 x)" );
			
			var a = 0;
		}

		public mysToken ParseLex( string s ) {
			bool quote = false;
			mysToken token = null;

			if ( s[ 0 ] == '\'' ) {
				quote = true;
				s = s.Substring( 1, s.Length - 1 );
			}

			if ( s.All( c => c >= '0' && c <= '9' ) ) {
				token = new mysIntegral( long.Parse( s ) );
			} else if ( s.All( c =>
				c == '.' || c == ',' ||
				(c >= '0' && c <= '9' ) )
			) {
				s = s.Replace( '.', ',' );
				token = new mysFloating( double.Parse( s ) );
			} else if ( s[ 0 ] == '(' ) {
				token = Parse( s.Substring( 1, s.Length - 2 ) );
			} else {
				token = new mysSymbol( s );
			}

			//if ( token == null )
				//throw new ArgumentException();

			if ( quote ) {
				token.Quote();
			}

			return token;
		}

		public mysList Parse( string s ) {
			int listDepth = 0;

			s = s.Trim();

			mysList list = new mysList();

			List<string> pieces = new List<string>();

			int lastSplit = 0;
			string piece = "";

			for ( int i = 0; i < s.Length; i++ ) {
				if ( s[ i ] == '(' ) {
					listDepth++;
					lastSplit = i;

					if ( s[ i - 1] == '\'' ) {
						lastSplit--;
					}
				}

				if ( s[ i ] == ')' ) {
					listDepth--;
					pieces.Add( s.Substring( lastSplit, 1 + i - lastSplit ) );
					lastSplit = i + 1;
				}

				if ( listDepth == 0 && s[ i ] == ' ' ) {
					piece = s.Substring( lastSplit, i - lastSplit );

					if ( piece != "" ) {
						pieces.Add( piece );
						list.InternalValues.Add( ParseLex( piece ) );
					}

					lastSplit = i + 1;
				}
			}

			piece = s.Substring( lastSplit, s.Length - lastSplit );

			if ( piece != "" ) {
				pieces.Add( piece );
				list.InternalValues.Add( ParseLex( piece ) );
			}

			//return null;
			return list;
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
