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

	public enum mysTypes {
		Symbol,
		Integral,
		Floating,
		List,
		FunctionGroup,
		mysType
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
		}

		public void TestFunction() {
			// test stuff below

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
