using System;
using System.Collections.Generic;
using System.Linq;

namespace mysharp
{
	class NoSuchSignatureException : Exception { }

	class Program
	{
		static void Main(string[] args)
		{
			mysREPL repl = new mysREPL();
		}
	}

	class mysREPL
	{
		Dictionary<string, mysSymbolSpace> nameSpaces;
		Stack<mysSymbolSpace> spaceStack;

		public mysREPL() {
			nameSpaces = new Dictionary<string, mysSymbolSpace>();
			nameSpaces.Add( "global" , new mysSymbolSpace() );

			spaceStack = new Stack<mysSymbolSpace>();

			mysBuiltins.Setup();

			// test stuff below

			mysSymbol addition = nameSpaces[ "global" ].Create( "+" );
			nameSpaces[ "global" ].Define( addition, mysBuiltins.Addition );

			List<mysToken> testExpression = new List<mysToken>();

			mysFunctionGroup fg = new mysFunctionGroup();
			mysFunction f = new mysFunction();

			f.Symbols.Add( new mysSymbol( "x" ) );
			f.Signature.Add( mysTypes.Integral );
			f.Symbols.Add( new mysSymbol( "y" ) );
			f.Signature.Add( mysTypes.Integral );

			fg.Variants.Add( f );

			testExpression.Add( EvaluateSymbol( addition ) );
			testExpression.Add( EvaluateSymbol( addition ) );
			testExpression.Add( new mysIntegral( 1 ) );
			testExpression.Add( new mysIntegral( 2 ) );
			testExpression.Add( new mysIntegral( 3 ) );

			mysList expression = new mysList( testExpression );
			expression.Evaluate( new Stack<mysSymbolSpace>( spaceStack ) );
		}

		mysToken EvaluateSymbol( mysSymbol symbol ) {
			Stack<mysSymbolSpace> evaluationStack =
				new Stack<mysSymbolSpace>( spaceStack );

			while ( evaluationStack.Count > 0 ) {
				mysSymbolSpace space = evaluationStack.Pop();
				if ( space.Exists( symbol ) ) {
					return space.GetValue( symbol );
				}
			}

			// check global
			if ( nameSpaces[ "global" ].Exists( symbol ) ) {
				return nameSpaces[ "global" ].GetValue( symbol );
			}

			throw new ArgumentException( "Symbol isn't defined." );
		}
	}

	public class mysSymbolSpace
	{
		private Dictionary<string, mysSymbol> symbols =
			new Dictionary<string, mysSymbol>();

		public mysSymbol Create( string symbolString ) {
			if ( symbols.ContainsKey( symbolString ) ) {
				throw new ArgumentException("Symbol already exists.");
			}

			mysSymbol newSymbol = new mysSymbol( symbolString );
			symbols.Add( symbolString, newSymbol );

			return newSymbol;
		}

		public mysSymbol Get( string symbolString ) {
			return symbols[ symbolString ];
		}

		public bool Exists( mysSymbol symbol ) {
			return Values.ContainsKey( symbol );
		}

		public void Define( mysSymbol symbol, mysToken value ) {
			Values.Add( symbol, value );
		}

		public mysToken GetValue( mysSymbol symbol ) {
			return Values[ symbol ];
		}

		Dictionary<mysSymbol, mysToken> Values;

		public mysSymbolSpace() {
			Values = new Dictionary<mysSymbol, mysToken>();
		}
	}

	public class mysNameSpace
	{
		public mysSymbolSpace SymbolSpace;

		public mysNameSpace() {
			SymbolSpace = new mysSymbolSpace();
		}
	}

	public class mysToken
	{
		public mysTypes Type;
	}

	// lh: should (/must) be a token later again
	//     it just got a bit confusing for a while
	public class mysSymbol// : mysToken
	{
		private string stringRepresentation;

		public mysSymbol( string symbolString ) {
			stringRepresentation = symbolString;
		}
	}

	class mysList : mysToken
	{
		public List<mysToken> InternalValues;

		public mysList() {
			InternalValues = new List<mysToken>();
		}

		public mysList( List<mysToken> list ) {
			InternalValues = new List<mysToken>( list );
		}

		public List<mysToken> Evaluate(
			//List<mysToken> expression
			Stack<mysSymbolSpace> spaceStack
		) {
			Queue<mysToken> queue = new Queue<mysToken>();
			List<mysToken> currentExpression =
				new List<mysToken>( InternalValues );

			while ( true ) {
				mysToken last;
				int currentLast = currentExpression.Count - 1;

				while (
					currentLast >= 0 &&
					currentExpression.Count > currentLast
				) {
					last = currentExpression.ElementAt( currentLast );
					
					if ( last.Type == mysTypes.FunctionGroup ) {
						mysFunctionGroup fg = last as mysFunctionGroup;

						List<mysToken> passedArgs = queue.Reverse().ToList();

						while ( passedArgs.Count > 0 ) {
							mysFunction matching = fg.Judge( passedArgs );

							//if ( fg.Judge( passedArgs ) != null ) {
							if ( matching != null ) {
								// remove the now evaluated bit from the expr
								currentExpression.RemoveRange(
									currentLast, passedArgs.Count + 1
								);

								// call function and add our result back into
								// the expression
								if ( matching is mysBuiltin ) {
									currentExpression.Insert(
										currentLast,
										(matching as mysBuiltin).Call(
											new Stack<mysSymbolSpace>( spaceStack ),
											passedArgs
										)
									);
								} else {
									currentExpression.Insert(
										currentLast,
										matching.Call(
											new Stack<mysSymbolSpace>( spaceStack ),
											passedArgs
										)
									);
								}

								// automatically gets decremented outside of
								// this while loop.
								currentLast = currentExpression.Count;
								queue.Clear();
								break;
							} else {
								if ( passedArgs.Count > 0 ) {
									// remove last, try again.
									passedArgs.RemoveAt( passedArgs.Count - 1 );
								} else {
									throw new NoSuchSignatureException();
								}
							}
						}
					} else {
						queue.Enqueue( last );
					}

					//queue.Enqueue( last );
					//currentExpression.RemoveAt( currentExpression.Count - 1 );
					currentLast--;
				}

				break;
			}

			return queue.Reverse().ToList();
		}
	}

	public enum mysTypes {
		Integral,
		Floating,
		List,
		FunctionGroup
	}

	public class mysFunctionGroup : mysToken
	{
		// lh: A function group is a collection of functions assigned the same
		//     symbol, but with different signatures.

		public List<mysFunction> Variants;

		public mysFunctionGroup() {
			Type = mysTypes.FunctionGroup;
			Variants = new List<mysFunction>();
		}

		// lh: returns a matching function, or null if we didn't like the input.
		public mysFunction Judge( List<mysToken> arguments ) {

			List<mysFunction> variants = new List<mysFunction>( Variants );
			// lh: make this a bit cleverer later to handle variadics.
			variants.RemoveAll( v => v.SignatureLength != arguments.Count );

			// lh: make sure the types of our sig match perfectly with the types
			//     of the arguments
			variants.RemoveAll( v =>
				v.Signature
					.Zip( arguments, (va, a) => va == a.Type )
					.Where( p => p )
					.Count() != arguments.Count
			);

			if ( variants.Count != 1 ) {
				return null;
			}

			return variants[ 0 ];
		}
	}

	public static class Extensions
	{
		public static void DoPaired<T, U>(
			this IEnumerable<T> t,
			IEnumerable<U> other,
			Action<T, U> action
		) {
			for ( int i = 0; i < Math.Min( t.Count(), other.Count() ); i++ ) {
				action( t.ElementAt( i ), other.ElementAt( i ) );
			}
		}
	}

	public class mysFunction
	{
		// lh: A function is in essence just a list that we interpret as a
		//     parseblock when the function is called upon, making sure to
		//     substitute in our passed values.

		public List<mysTypes> Signature;
		public List<mysSymbol> Symbols;

		public int SignatureLength {
			get { return Signature.Count; }
		}

		public List<mysToken> Function;

		public mysFunction() {
			Signature = new List<mysTypes>();
			Symbols = new List<mysSymbol>();
			Function = new List<mysToken>();
		}

		public virtual mysToken Call(
			Stack<mysSymbolSpace> spaceStack,
			List<mysToken> arguments
		) {
			// future, cache somehow
			mysSymbolSpace internalSpace = new mysSymbolSpace();

			Symbols.DoPaired(
				arguments,
				(s, a) => internalSpace.Define( s, a )
			);

			spaceStack.Push( internalSpace );

			// evaluate

			// lh: until we have userdefined functions in
			return new mysIntegral( 0 );
		}
	}

	public static class mysBuiltins {
		public static mysFunctionGroup Addition;

		public static void Setup() {
			Addition = new mysFunctionGroup();

			mysBuiltin addition = new mysBuiltin();
			addition.Signature.Add( mysTypes.Integral );
			addition.Signature.Add( mysTypes.Integral );

			addition.Function = new Func<List<mysToken>, mysToken>(
				args =>
					new mysIntegral(
						(args[ 0 ] as mysIntegral).Value +
						(args[ 1 ] as mysIntegral).Value
					)
			);

			Addition.Variants.Add( addition );
		}
	}

	public class mysBuiltin : mysFunction {
		public new Func<List<mysToken>, mysToken> Function;

		// not sure we need to override? but I'm not chancing
		public override mysToken Call(
			Stack<mysSymbolSpace> spaceStack,
			List<mysToken> arguments
		) {
			return Function( arguments );
		}
	}

	class mysIntegral : mysToken
	{
		public long Value;

		public mysIntegral( long value ) {
			Type = mysTypes.Integral;
			Value = value;
		}
	}

	class mysFloating : mysToken
	{
		double Value;

		public mysFloating( double value ) {
			Type = mysTypes.Floating;
			Value = value;
		}
	}
}
