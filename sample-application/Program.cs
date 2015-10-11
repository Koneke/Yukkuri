using System.Reflection;

namespace sample_application
{
	public class SampleClass
	{
		public static int Bar = 13;
		public static float Test = 1.32f;
		public static double Dest = 14.2;

		public int AField;

		public Foo Foo;

		public SampleClass() {
			AField = 5;
			Foo = new Foo();
		}

		public SampleClass( int foop ) {
			AField = foop;
			Foo = new Foo();
		}

		public int AMethod() {
			return AField;
		}
	}

	public class Foo
	{
		public int BField;

		public Foo() {
			BField = 10;
		}

		public override string ToString()
		{
			return "test";
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			mysharp.mysREPL REPL = new mysharp.mysREPL();

			REPL.ExposeTo( Assembly.GetExecutingAssembly() );
			REPL.ExposeTo( Assembly.GetAssembly( typeof( System.Console ) ) );

			Foo f = new Foo();
			f.BField = -1;

			REPL.Evaluate(
				"def 'a {0}",
				f
			);

			int a = 10;
			double d = (double)a;

			REPL.REPLloop();
		}
	}
}
