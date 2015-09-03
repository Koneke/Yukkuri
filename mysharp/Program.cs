using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

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
		// just for ease of access/reading
		public mysSymbolSpace Global;

		public Dictionary<string, mysSymbolSpace> nameSpaces;

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

		mysParser parser;
		bool strict;

		// loop loop, I know...
		public void REPLloop() {
			bool quit = false;
			strict = false;
			string accumInput = "";

			while ( !quit ) {
				Console.Write(
					accumInput == ""
					? " > "
					: " . "
				);
				string input = Console.ReadLine();

				switch ( input ) {
					case "quit":
					case "(quit)":
						quit = true;
						break;

					case "strict":
					case "(strict)":
						strict = !strict;
						Console.WriteLine( "Strict is now {0}.\n", strict );
						break;

					default:
						accumInput += input;

						if ( input.Last() == '\\') {
							break;
						}

						accumInput = accumInput
							.Replace( '\\', ' ' )
							.Replace( "\t", "" )
						;

						List<mysToken> output = Evaluate( accumInput );

						string outputstring = string.Join( ", ", output );
						if ( outputstring != "" ) {
							Console.WriteLine( outputstring );
						}

						Console.WriteLine( "Ok.\n" );

						accumInput = "";
						break;
				}
			}
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
