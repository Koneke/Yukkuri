using System;
using System.Collections.Generic;
using System.Linq;

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
			mysToken
		> Function;

		// not sure we need to override? but I'm not chancing
		public override mysToken Call(
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

			mysToken result = Function( arguments, state, spaceStack );

			spaceStack.Pop();

			// shouldn't functions possible return a list of tokens, instead of
			// just one..? if so, we'll need to make this return firstordefault
			// or a new list here, like in mysFunction.
			return result;
		}
	}
}
