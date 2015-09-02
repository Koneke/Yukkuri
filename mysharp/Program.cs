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

			var a = 0;
		}

		public void TestFunction() {
			mysParser parser = new mysParser();

			mysToken result;
			mysList parsed;

			parsed = parser.Parse(
				"+ 1 3"
			);
			result = parsed.Evaluate( spaceStack );

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
	}
}
