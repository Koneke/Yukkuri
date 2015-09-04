using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

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
		// I don't think global/general namespace stuff should be in the repl?
		// the repl should probably just be our "front end", so to speak

		// just for ease of access/reading
		public mysSymbolSpace Global;

		public Dictionary<string, mysSymbolSpace> nameSpaces;

		mysParser parser;
		bool quit;
		bool strict;
		string accumulatedInput;

		// not sure if we actually need this in the repl...?
		// might be enough to just pass in global
		public Stack<mysSymbolSpace> spaceStack;

		public mysREPL() {
			parser = new mysParser();

			nameSpaces = new Dictionary<string, mysSymbolSpace>();

			Global = new mysSymbolSpace();
			nameSpaces.Add( "global" , Global );

			spaceStack = new Stack<mysSymbolSpace>();
			spaceStack.Push( Global );

			mysBuiltins.Setup( Global );
		}

		public List<mysToken> Evaluate( string expression ) {
			List<mysToken> parsed = parser.Parse( expression );

			try {
				EvaluationMachine em = new EvaluationMachine(
					parsed,
					spaceStack
				);
				List<mysToken> output = em.Evaluate();

				return output;
			} catch (Exception e) when ( !strict ) {
				Console.WriteLine( e.Message + "\n" );
				return null;
			}
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
			foreach(Type t in a.GetTypes() ) {
				Console.WriteLine( t.FullName );
			}

			Type type = a.GetType( "sample_application.SampleClass" );

			foreach ( FieldInfo fi in type.GetFields() ) {
				Console.WriteLine( "Field: " + fi.Name );
			}

			foreach ( MethodInfo mi in type.GetMethods() ) {
				Console.WriteLine( "Method: " + mi.Name );
			}

			MethodInfo m = type.GetMethod( "AMethod" );
			dynamic obj = Activator.CreateInstance( type );
			object result = m.Invoke( obj, new object[] { } );

			if ( result is int ) {
				;
			}

			;
		}
	}
}
