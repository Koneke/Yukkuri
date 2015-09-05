using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace mysharp
{
	class NoSuchSignatureException : Exception
	{
		public NoSuchSignatureException( string message )
			: base( message )
		{
		}
	}

	class SignatureAmbiguityException : Exception {
		public SignatureAmbiguityException()
		{
		}

		public SignatureAmbiguityException( string message )
			: base( message )
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

		public static T Car<T>(
			this IEnumerable<T> enumerable
		) {
			return enumerable.FirstOrDefault();
		}

		public static IEnumerable<T> Cdr<T>(
			this IEnumerable<T> enumerable
		) {
			return enumerable.Skip( 1 );
		}

		public static string StringJoin<T>(
			this IEnumerable<T> enumerable,
			string separator = ""
		) {
			return string.Join( separator, enumerable );
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			mysREPL repl = new mysREPL();
			repl.REPLloop();
		}
	}

	public class mysREPL
	{
		public mysState State;

		mysParser parser;
		bool quit;
		bool strict;
		string accumulatedInput;

		public mysREPL() {
			parser = new mysParser();
			State = new mysState();
		}

		// loop loop, I know...
		public void REPLloop() {
			quit = false;
			strict = false;
			accumulatedInput = "";

			while ( !quit ) {
				// show standard prompt > if we are not currently continuing a
				// multiline command, else show the continuation prompt .
				Console.Write(
					accumulatedInput.Count() == 0
					? " > "
					: " . "
				);

				string input = Console.ReadLine();

				switch ( input ) {
					case "(clear)":
						Console.Clear();
						break;

					case "(quit)":
						quit = true;
						break;

					case "(strict)":
						strict = !strict;
						Console.WriteLine( "Strict is now {0}.\n", strict );
						break;

					default:
						handleInput( input );
						break;
				}
			}
		}

		public List<mysToken> Evaluate( string expression ) {
			List<mysToken> parsed = parser.Parse( expression );

			try {
				return State.Evaluate( parsed );

			} catch (Exception e) when ( !strict ) {
				Console.WriteLine( e.Message + "\n" );
				return null;
			}
		}

		void handleInput( string input ) {
			if ( input.Last() == '\\') {
				input = input.Substring( 0, input.Length - 1);
				accumulatedInput += input;
				return;
			} else {
				accumulatedInput += input;
			}

			accumulatedInput = accumulatedInput
				.Replace( "\t", " " )
			;

			List<mysToken> output = Evaluate( accumulatedInput );

			if ( output != null ) {
				string outputstring = string.Join( ", ", output );

				if ( outputstring != "" ) {
					Console.WriteLine( outputstring );
				}

				Console.WriteLine( "Ok.\n" );
			}

			accumulatedInput = "";
		}

		public void ExposeTo( Assembly a ) {
			State.ExposeTo( a );
		}
	}
}
