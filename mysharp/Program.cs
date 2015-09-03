using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace mysharp
{
	class NoSuchSignatureException : Exception
	{
		public NoSuchSignatureException(string message) : base(message)
		{
		}
	}

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

		public static IEnumerable<T> Between<T>(
			this IEnumerable<T> enumerable,
			int first,
			int count
		) {
			return enumerable
				.Skip( first )
				.Take( count );
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			mysREPL repl = new mysREPL();
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

					try {
						EvaluationMachine em = new EvaluationMachine();
						List<mysToken> output = em.Evaluate(
							parsed,
							spaceStack
						);

						string outputstring = string.Join( ", ", output );
						if ( outputstring != "" ) {
							Console.WriteLine( outputstring );
						}

						Console.WriteLine( "Ok.\n" );
					} catch (Exception e) {
						Console.WriteLine( e.Message + "\n" );
					}
				}
			}
		}
	}
}
