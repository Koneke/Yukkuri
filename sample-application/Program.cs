using System.Reflection;

namespace sample_application
{
	public class SampleClass
	{
		public int AField;

		public Foo Foo;

		public SampleClass() {
			AField = 5;
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
	}

	class Program
	{
		static void Main(string[] args)
		{
			mysharp.mysREPL REPL = new mysharp.mysREPL();

			REPL.ExposeTo( Assembly.GetExecutingAssembly() );
			REPL.ExposeTo( Assembly.GetAssembly( typeof( System.Console ) ) );

			REPL.REPLloop();
		}
	}
}
