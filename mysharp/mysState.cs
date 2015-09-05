using System;
using System.Reflection;
using System.Collections.Generic;

namespace mysharp
{
	// persistent information about our execution context,
	// i.e. stuff like "what have we imported", "what namespaces exist",
	// etc.
	// execution context might be a good alternate name for this
	public class mysState
	{
		public mysSymbolSpace Global;
		public Dictionary<string, mysSymbolSpace> nameSpaces;
		public Dictionary<string, Assembly> exposedAssemblies;

		public mysState() {
			nameSpaces = new Dictionary<string, mysSymbolSpace>();
			exposedAssemblies = new Dictionary<string, Assembly>();

			Global = new mysSymbolSpace();
			nameSpaces.Add( "global" , Global );

			mysBuiltins.Setup( Global );
		}

		public List<mysToken> Evaluate( List<mysToken> expression ) {
			Stack<mysSymbolSpace> spaceStack = new Stack<mysSymbolSpace>();
			spaceStack.Push( Global );

			EvaluationMachine em = new EvaluationMachine(
				expression,
				spaceStack
			);
			List<mysToken> output = em.Evaluate();

			return output;
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
