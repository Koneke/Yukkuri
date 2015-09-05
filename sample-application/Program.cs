using System.Reflection;

namespace sample_application
{
	public class SampleClass
	{
		public int AField;

		public int AMethod() {
			return AField;
		}

		public SampleClass() {
			AField = 5;
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			mysharp.mysREPL REPL = new mysharp.mysREPL();
			REPL.ExposeTo( Assembly.GetExecutingAssembly() );

			REPL.Evaluate( "(#new \"sample_application.SampleClass\")" );

			System.Console.ReadLine();
		}
	}
}
