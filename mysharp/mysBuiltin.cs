using System;
using System.Linq;
using System.Collections.Generic;

namespace mysharp
{
	public class mysBuiltin : mysFunction {
		public static void AddVariant(
			string name,
			mysFunction variant,
			mysSymbolSpace global
		) {
			mysSymbol symbol = new mysSymbol( name );
			mysFunctionGroup fg;

			if ( !global.Defined( symbol ) ) {
				global.Define(
					symbol,
					new mysToken( new mysFunctionGroup() )
				);
			}

			fg = global
				.GetValue( new mysSymbol( name ) )
				.InternalValue
				as mysFunctionGroup;

			fg.Variants.Add( variant );
		}

		public static void DefineInGlobal(
			string name,
			mysFunctionGroup fg,
			mysSymbolSpace global
		) {
			mysSymbol symbol = global.Create( name );
			global.Define( symbol, new mysToken( fg ) );
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
				t.Type == typeof(mysSymbol) && !t.Quoted
				? ( t.InternalValue as mysSymbol ).Value( spaceStack )
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
}
