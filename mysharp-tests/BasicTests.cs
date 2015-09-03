using NUnit.Framework;

namespace mysharp_tests
{
	using mysharp;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;

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
				result.Type == mysTypes.Integral &&
				( result as mysIntegral ).Value == expected,
				$"Failing at {expression}."
			);
		}

		void Test(
			mysREPL REPL,
			string expression,
			System.Func<mysToken, bool> evaluation
		) {
			// might want to not autocar this, but it is useful right now
			mysToken result = REPL.Evaluate( expression ).Car();

			Debug.Assert(
				evaluation( result ),
				$"Failing at {expression}."
			);
		}

		[Test]
		public void IntegralTests() {
			long testValue = 10;

			mysSymbol x = new mysSymbol( "x" );

			mysharp.mysREPL REPL = new mysREPL();
			REPL.Global.Define( x, new mysIntegral( testValue ) );

			mysSymbol x2 = new mysSymbol( "x" );

			Debug.Assert(
				REPL.Global.Defined( x ),
				"Symbol \"x\" not defined."
			);

			Debug.Assert(
				REPL.Global.GetValue( x ).Type == mysTypes.Integral,
				"\"x\" not reported as integral-type."
			);

			mysIntegral i = REPL.Global.GetValue( x ) as mysIntegral;
			
			Debug.Assert(
				i != null,
				"\"x\" reported as but not actually integral-type."
			);

			Debug.Assert(
				i.Value == testValue,
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
				result.Type == mysTypes.Integral,
				"f not returning :int."
			);

			Debug.Assert(
				( result as mysIntegral ).Value == 5,
				"f not returning correct value."
			);

			REPL.Evaluate( "(def 'g (=> [x :int] [+ 2 (f x)]))" );
			result = REPL.Evaluate( "(g 2)" ).Car();

			Debug.Assert(
				result.Type == mysTypes.Integral,
				"g not returning :int."
			);

			Debug.Assert(
				( result as mysIntegral ).Value == 7,
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
	}
}
