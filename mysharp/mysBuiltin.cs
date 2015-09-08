using System;
using System.Linq;
using System.Collections.Generic;

namespace mysharp
{
	public class mysBuiltin : mysFunction {
		public static void DefineInGlobal(
			string name,
			mysFunctionGroup fg,
			mysSymbolSpace global
		) {
			mysSymbol symbol = global.Create( name );
			symbol.Type = mysTypes.FunctionGroup;
			fg.Type = mysTypes.FunctionGroup;
			
			global.Define( symbol, fg );
		}
		
		public new Func<
			List<mysToken>,
			mysState,
			Stack<mysSymbolSpace>,
			List<mysToken>
		> Function;

		// not sure we need to override? but I'm not chancing
		public override List<mysToken> Call(
			List<mysToken> arguments,
			mysState state,
			Stack<mysSymbolSpace> spaceStack
		) {
			arguments = arguments.Select( t =>
				t.Type == mysTypes.Symbol && !t.Quoted
				? ( t as mysSymbol ).Value( spaceStack )
				: t
			).ToList();

			// do we need to push our own internal space here?
			// I mean, maybe?

			mysSymbolSpace internalSpace = new mysSymbolSpace();

			spaceStack.Push( internalSpace );

			List<mysToken> result = Function( arguments, state, spaceStack );

			spaceStack.Pop();

			// shouldn't functions possible return a list of tokens, instead of
			// just one..? if so, we'll need to make this return firstordefault
			// or a new list here, like in mysFunction.
			return result;
		}
	}

	public class clrFunction : mysFunction {
		//# Print System.Console
		//# Print (#. System Console)
		//# WriteLine (. [System Console]) "hi!"

		public new string Function;

		// arg 1 function
		// arg 2 instance (with . operator)
		// arg 3+ arguments
		public override List<mysToken> Call(
			List<mysToken> arguments,
			mysState state,
			Stack<mysSymbolSpace> spaceStack
		) {
			mysSymbol symbol = arguments[ 0 ] as mysSymbol;
			clrObject obj = arguments[ 1 ] as clrObject;

			return base.Call(arguments, state, spaceStack);
		}
	}
}
