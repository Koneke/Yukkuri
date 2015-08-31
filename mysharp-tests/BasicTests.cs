using NUnit.Framework;

namespace mysharp_tests
{
	using mysharp;
	using System.Diagnostics;

	[TestFixture]
	public class BasicTests
	{
		[Test]
		public void IntegralTest() {
			long testValue = 10;

			mysSymbol x = new mysSymbol( "x" );

			mysharp.mysREPL REPL = new mysREPL();
			REPL.Global.Define( x, new mysIntegral( testValue ) );

			mysSymbol x2 = new mysSymbol( "x" );

			Debug.Assert(
				REPL.Global.Defined( x ),
				"Symbol \"x\" not defined."
			);

			System.Console.WriteLine(
				"x-type: {0}", REPL.Global.GetValue( x ).Type.ToString()
			);

			/*Debug.Assert(
				REPL.Global.GetValue( x ).Type != mysTypes.Integral,
				"\"x\" not reported as integral-type."
			);*/

			mysIntegral i = REPL.Global.GetValue( x ) as mysIntegral;
			
			Debug.Assert(
				i != null,
				"\"x\" reported as but not actually integral-type."
			);

			Debug.Assert(
				i.Value != testValue,
				"Value of \"x\" doesn't match testValue."
			);
		}
	}
}
