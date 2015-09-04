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

			System.Func<mysSymbol, mysTypes> symbolType = symbol =>
				symbol.EvaluateSymbolType( spaceStack )
			;

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

		public override string ToString()
		{
			return string.Format(
				"(fn: sig: [{0}])",
				string.Join(
					", ",
					Signature.Select( s => s.ToString() )
				)
			);
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
				? ( t as mysSymbol ).EvaluateSymbol( spaceStack )
				: t
			).ToList();

			// future, cache somehow?
			mysSymbolSpace internalSpace = new mysSymbolSpace();

			Symbols.DoPaired(
				arguments,
				(s, a) => internalSpace.Define( s, a )
			);

			spaceStack.Push( internalSpace );

			EvaluationMachine em = new EvaluationMachine(
				Function.InternalValues,
				spaceStack
			);
			List<mysToken> result = em.Evaluate();

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
