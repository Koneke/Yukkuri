using System.Linq;
using System.Collections.Generic;

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

			variants.RemoveAll(
				v => !judgeVariant( v, arguments, spaceStack )
			);

			if ( variants.Count == 0 ) {
				return null;
			} else if ( variants.Count != 1 ) {
				throw new SignatureAmbiguityException();
			}

			return variants[ 0 ];
		}

		bool judgeVariant(
			mysFunction variant,
			List<mysToken> arguments,
			Stack<mysSymbolSpace> spaceStack
		) {
			// lh: make this a bit cleverer later to handle variadics.
			if ( variant.SignatureLength != arguments.Count ) {
				return false;
			}

			if ( variant.Signature
				.Zip(
					arguments,
					(type, token) => typeCheck( type, token, spaceStack )
				)
				// if the zip of our two collections is less than the count
				// we started with, at least one given token did not match
				// the sig, so we remove that variant from the potential
				// ones.
				.Where( p => p )
				.Count() != arguments.Count
			) {
				return false;
			}

			return true;
		}

		// given a type from our sig, and the token supplied as a potential
		// argument, see if they match
		bool typeCheck(
			mysTypes type,
			mysToken token,
			Stack<mysSymbolSpace> spaceStack
		) {
			// we might need to do weird shit with symbols in here?
			// like, you should only really be able to send a quoted symbol
			// in (mainly for ease of reading the code, easy to see reasoning
			// etc., less likely for bugs to occur because of an accidental sig
			// match).

			return 
				type == token.Type ||
				type == mysTypes.ANY ||
				// massive c#6 boner right here
				type == ( token as mysSymbol )?.DeepType( spaceStack )
			;
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
