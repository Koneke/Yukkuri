﻿using System;
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
		public List<Assembly> exposedAssemblies;

		public mysState() {
			nameSpaces = new Dictionary<string, mysSymbolSpace>();
			exposedAssemblies = new List<Assembly>();

			Global = new mysSymbolSpace();

			Global.Define(
				Global.Create( "true" ),
				new mysToken( true )
			);

			Global.Define(
				Global.Create( "false" ),
				new mysToken( false )
			);

			nameSpaces.Add( "global" , Global );

			mysBuiltins.Setup( Global );
		}

		public mysToken Evaluate( List<mysToken> expression ) {
			Stack<mysSymbolSpace> spaceStack = new Stack<mysSymbolSpace>();
			spaceStack.Push( Global );

			return new EvaluationMachine(
				expression,
				this,
				spaceStack
			).Evaluate();
		}

		public void ExposeTo( Assembly a ) {
			exposedAssemblies.Add( a );
		}
	}
}
