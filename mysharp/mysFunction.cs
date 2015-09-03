using System.Collections.Generic;
using System.Linq;

namespace mysharp
{
	public class mysFunctionGroup : mysToken
	{
		// lh: A function group is a collection of functions assigned the same
		//     symbol, but with different signatures.

		public List<mysFunction> Variants;

		public mysFunctionGroup() {
			Variants = new List<mysFunction>();
		}

		// lh: returns a matching function, or null if we didn't like the input.
		public mysFunction Judge(
			List<mysToken> arguments,
			Stack<mysSymbolSpace> spaceStack
		) {

			List<mysFunction> variants = new List<mysFunction>( Variants );
			// lh: make this a bit cleverer later to handle variadics.
			variants.RemoveAll( v => v.SignatureLength != arguments.Count );

			System.Func<mysSymbol, mysTypes> symbolType = 
				symbol =>
					EvaluationMachine.EvaluateSymbolType(
						symbol,
						spaceStack
					);

			System.Func<mysTypes, mysToken, bool> typeCheck =
				(type, token) =>
					type == token.Type ||
					type == mysTypes.ANY ||
					( token.Type == mysTypes.Symbol &&
					  symbolType( token as mysSymbol ) == type)
			;

			// lh: make sure the types of our sig match perfectly with the types
			//     of the arguments
			variants.RemoveAll( v =>
				v.Signature
					// va = type expected
					// a.Type = type received
					.Zip(
						arguments,
						typeCheck
					)
					// find the ones where previous comparison was true
					.Where( p => p )
					// make sure the count is right
					.Count() != arguments.Count
			);

			if ( variants.Count != 1 ) {
				return null;
			}

			return variants[ 0 ];
		}
	}

	public class mysFunction : mysToken
	{
		// lh: A function is in essence just a list that we interpret as a
		//     parseblock when the function is called upon, making sure to
		//     substitute in our passed values.

		public mysTypes ReturnType;
		public List<mysTypes> Signature;
		public List<mysSymbol> Symbols;

		public int SignatureLength {
			get { return Signature.Count; }
		}

		public mysList Function;

		public mysFunction() {
			Type = mysTypes.Function;

			Signature = new List<mysTypes>();
			Symbols = new List<mysSymbol>();

			Function = new mysList();
		}

		public virtual mysToken Call(
			Stack<mysSymbolSpace> spaceStack,
			List<mysToken> arguments
		) {
			arguments = arguments.Select( t =>
				t.Type == mysTypes.Symbol && !t.Quoted
				? EvaluationMachine.EvaluateSymbol(
					t as mysSymbol,
					spaceStack)
				: t
			).ToList();

			// future, cache somehow?
			mysSymbolSpace internalSpace = new mysSymbolSpace();

			Symbols.DoPaired(
				arguments,
				(s, a) => internalSpace.Define( s, a )
			);

			spaceStack.Push( internalSpace );

			EvaluationMachine em = new EvaluationMachine();
			List<mysToken> result = em.Evaluate(
				Function.InternalValues,
				spaceStack
			);

			spaceStack.Pop();

			if ( result.Count < 2 ) {
				return result.FirstOrDefault();
			} else {
				// maybe quoted..? unsure atm
				return new mysList( result );
			}
		}
	}
}
