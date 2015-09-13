using System.Linq;
using System.Collections.Generic;

namespace mysharp
{
	public class mysFunction : mysToken
	{
		// lh: A function is in essence just a list that we interpret as a
		//     parseblock when the function is called upon, making sure to
		//     substitute in our passed values.

		// not used right now, but should be later
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

		public mysFunction()
			: base ( null, mysTypes.Function )
		{
			Signature = new List<mysTypes>();
			Symbols = new List<mysSymbol>();

			// consider making this the "main value" of the token
			Function = new mysList();
		}

		public virtual List<mysToken> Call(
			List<mysToken> arguments,
			mysState state,
			Stack<mysSymbolSpace> spaceStack
		) {
			arguments = arguments.Select( t =>
				t.Type == mysTypes.Symbol && !t.Quoted
				? ( t as mysSymbol ).Value( spaceStack )
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
				state,
				spaceStack
			);
			List<mysToken> result = em.Evaluate();

			spaceStack.Pop();

			return result;
		}
	}
}
