using System.Collections.Generic;
using System.Linq;

namespace mysharp
{
	public class mysFunctionGroup : mysToken
	{
		// lh: A function group is a collection of functions assigned the same
		//     symbol, but with different signatures.

		public List<mysFunction> Variants;

		public mysFunctionGroup()
			: base ( null, mysTypes.FunctionGroup )
		{
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
				type == ( token as mysSymbol )?.DeepType( spaceStack )
			;
		}
	}
}
