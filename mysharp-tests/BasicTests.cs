using NUnit.Framework;

namespace mysharp_tests
{
	using mysharp;
	using System.Collections.Generic;
	using System.Diagnostics;

	public static class TestExtensions
	{
		public static void Test(
			this mysREPL REPL,
			string expression,
			System.Func<mysToken, bool> evaluation
		) {
			List<mysToken> result = REPL.Evaluate( expression );

			foreach( mysToken token in result ) {
				bool eval = evaluation( token );

				Assert.IsTrue(
					eval,
					$"Failing at {expression}."
				);
			}
		}
	}

	[TestFixture]
	public class BasicTests
	{
		void Test(
			mysREPL REPL,
			string expression,
			int expected
		) {
			mysToken result = REPL.Evaluate( expression ).Car();

			Debug.Assert(
				result.Type == typeof(int) &&
				(int)result.Value == expected,
				$"Failing at {expression}."
			);
		}

		[Test]
		public void IntegralTests() {
			int testValue = 10;

			mysSymbol x = new mysSymbol( "x" );

			mysharp.mysREPL REPL = new mysREPL();
			REPL.State.Global.Define( x, new mysToken( testValue ) );

			mysSymbol x2 = new mysSymbol( "x" );

			Debug.Assert(
				REPL.State.Global.Defined( x ),
				"Symbol \"x\" not defined."
			);

			Debug.Assert(
				REPL.State.Global.GetValue( x ).Type == typeof(int),
				"\"x\" not reported as integral-type."
			);

			mysToken i = REPL.State.Global.GetValue( x );
			
			Debug.Assert(
				i != null,
				"\"x\" reported as but not actually integral-type."
			);

			Debug.Assert(
				(int)i.Value == testValue,
				"Value of \"x\" doesn't match testValue."
			);
		}

		[Test]
		public void FunctionDefTests() {
			mysharp.mysREPL REPL = new mysREPL();
			mysToken result;

			REPL.Evaluate( "(def 'f (=> [x :int] [+ 3 x]))" );
			result = REPL.Evaluate( "(f 2)" ).Car();

			Debug.Assert(
				result.Type == typeof(int),
				"f not returning :int."
			);

			Debug.Assert(
				(int)result.Value == 5,
				"f not returning correct value."
			);

			REPL.Evaluate( "(def 'g (=> [x :int] [+ 2 (f x)]))" );
			result = REPL.Evaluate( "(g 2)" ).Car();

			Debug.Assert(
				result.Type == typeof(int),
				"g not returning :int."
			);

			Debug.Assert(
				(int)result.Value == 7,
				"g not returning correct value."
			);
		}

		[Test]
		public void ArithmeticTests() {
			mysharp.mysREPL REPL = new mysREPL();

			Test( REPL, "(+ 2 3)", 5 );
			Test( REPL, "(+ (+ 3 4) 3)", 10 );
			Test( REPL, "(- 4 3)", 1 );

			Test( REPL, "(- (+ 4 2) 3)", 3 );
			Test( REPL, "(- (- 4 2) 3)", -1 );
			Test( REPL, "(* 3 3)", 9 );

			Test( REPL, "(* (+ 2 1) 3)", 9 );
			Test( REPL, "(/ (+ 2 1) 3)", 1 );
			Test( REPL, "(/ 6 3)", 2 );
		}

		[Test]
		public void StringParsingTests() {
			mysharp.mysREPL REPL = new mysREPL();

			string quote = "\"";
			string escapedQuote = @"\" + "\"";

			(new mysREPL()).Test(
				quote + "foo" + quote,
				t => (string)t.Value == "foo"
			);

			(new mysREPL()).Test(
				quote + escapedQuote + "foo" + quote,
				t => (string)t.Value == ( "\"" + "foo" )
			);
		}
	}

	[TestFixture]
	public class ComparisonTests
	{
		[Test]
		public void Equality_EqualIntegers_True() {
			(new mysREPL()).Test(
				"(= 1 1)",
				t => (bool)t.Value == true
			);
		}

		[Test]
		public void Equality_NonEqualIntegers_False() {
			(new mysREPL()).Test(
				"(= 0 1)",
				t => (bool)t.Value == false
			);
		}

		[Test]
		public void Equality_EqualIntegerAFloatB_True() {
			(new mysREPL()).Test(
				"(= 1. 1)",
				t => (bool)t.Value == true
			);
		}

		[Test]
		public void Equality_NonEqualIntegerAFloatB_False() {
			(new mysREPL()).Test(
				"(= 1. 2)",
				t => (bool)t.Value == false
			);
		}

		[Test]
		public void Equality_NonEqualIntegerANonRoundFloatB_False() {
			(new mysREPL()).Test(
				"(= 1.1 1)",
				t => (bool)t.Value == false
			);
		}

		[Test]
		public void Equality_EqualFloats_True() {
			(new mysREPL()).Test(
				"(= 1.1 1.1)",
				t => (bool)t.Value == true
			);
		}

		[Test]
		public void Equality_NonEqualFloats_False() {
			(new mysREPL()).Test(
				"(= 1.1 1.2)",
				t => (bool)t.Value == false
			);
		}

		// ================================================

		[Test]
		public void GreaterThan_ALargerThanB_True() {
			(new mysREPL()).Test(
				"(> 1 0)",
				t => (bool)t.Value == true
			);
		}

		[Test]
		public void GreaterThan_ALargerThanNegativeB_True() {
			(new mysREPL()).Test(
				"(> 1 -1)",
				t => (bool)t.Value == true
			);
		}

		[Test]
		public void GreaterThan_NegativeALargerThanNegativeB_True() {
			(new mysREPL()).Test(
				"(> -1 -2)",
				t => ((bool)t.Value) == true
			);
		}

		[Test]
		public void GreaterThan_NegativeASmallerThanNegativeB_False() {
			(new mysREPL()).Test(
				"(> -2 -1)",
				t => (bool)t.Value == false
			);
		}

		[Test]
		public void GreaterThan_EqualIntegers_False() {
			(new mysREPL()).Test(
				"(> 1 1)",
				t => (bool)t.Value == false
			);
		}
	}
}
