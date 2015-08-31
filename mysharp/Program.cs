using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
			spaceStack.Push( nameSpaces[ "global" ] );

			mysBuiltins.Setup( nameSpaces[ "global" ] );

			// test stuff below

			List<mysToken> testExpression = new List<mysToken>();

			mysFunctionGroup fg = new mysFunctionGroup();
			mysFunction f = new mysFunction();

			f.Symbols.Add( new mysSymbol( "x" ) );
			f.Signature.Add( mysTypes.Integral );
			f.Symbols.Add( new mysSymbol( "y" ) );
			f.Signature.Add( mysTypes.Integral );

			f.Function.InternalValues.Add( EvaluateSymbol( GetSymbol( "+" ) ) );
			f.Function.InternalValues.Add( new mysSymbol( "x" ) );
			f.Function.InternalValues.Add( new mysSymbol( "y" ) );

			fg.Variants.Add( f );

			//testExpression.Add( EvaluateSymbol( GetSymbol( "+" ) ) );
			testExpression.Add( fg );
			testExpression.Add( new mysIntegral( 2 ) );
			testExpression.Add( new mysIntegral( 3 ) );

			mysList expression = new mysList( testExpression );
			mysToken result = expression.Evaluate(
				spaceStack
				//new Stack<mysSymbolSpace>( spaceStack )
			);

			var a = 0;
		}

		// not sure where exactly to put this fucking thing really
		// marking obsolete for now so we know that we're using the one in
		// REPL and not in list
		[Obsolete]
		mysToken EvaluateSymbol( mysSymbol symbol ) {
			Stack<mysSymbolSpace> evaluationStack =
				//new Stack<mysSymbolSpace>( spaceStack );
				spaceStack.Clone();

			while ( evaluationStack.Count > 0 ) {
				mysSymbolSpace space = evaluationStack.Pop();
				if ( space.Defined( symbol ) ) {
					return space.GetValue( symbol );
				}
			}

			throw new ArgumentException( "Symbol isn't defined." );
		}

		mysSymbol GetSymbol( string symbolString ) {
			Stack<mysSymbolSpace> evaluationStack = spaceStack.Clone();

			while ( evaluationStack.Count > 0 ) {
				mysSymbolSpace space = evaluationStack.Pop();

				if ( space.Exists( symbolString ) ) {
					return space.Get( symbolString );
				}
			}

			throw new ArgumentException( "Symbol doesn't exist." );
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

		public bool Exists( string symbolString ) {
			return symbols.ContainsKey( symbolString );
		}

		public void Define( mysSymbol symbol, mysToken value ) {
			Values.Add( symbol, value );
		}

		public bool Defined( mysSymbol symbol ) {
			return Values.ContainsKey( symbol );
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
	public class mysSymbol : mysToken
	{
		private string stringRepresentation;

		public mysSymbol( string symbolString ) {
			Type = mysTypes.Symbol;
			stringRepresentation = symbolString;
		}

		public override bool Equals(object obj)
		{
			if ( obj == null || obj.GetType() != GetType() )
				return false;

			mysSymbol s = (mysSymbol)obj;

			return s.stringRepresentation == stringRepresentation;
		}

		public override int GetHashCode()
		{
			return stringRepresentation.GetHashCode();
		}
	}

	public class mysList : mysToken
	{
		public List<mysToken> InternalValues;

		public mysList() {
			InternalValues = new List<mysToken>();
		}

		public mysList( List<mysToken> list ) {
			InternalValues = new List<mysToken>( list );
		}

		mysToken EvaluateSymbol(
			mysSymbol symbol,
			Stack<mysSymbolSpace> spaceStack
		) {
			Stack<mysSymbolSpace> evaluationStack = spaceStack.Clone();
				//new Stack<mysSymbolSpace>( spaceStack );

			while ( evaluationStack.Count > 0 ) {
				mysSymbolSpace space = evaluationStack.Pop();
				if ( space.Defined( symbol ) ) {
					return space.GetValue( symbol );
				}
			}

			throw new ArgumentException( "Symbol isn't defined." );
		}

		//public List<mysToken> Evaluate(
		public mysToken Evaluate(
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

					// evaluate symbols down
					while ( last.Type == mysTypes.Symbol ) {
						last = EvaluateSymbol( last as mysSymbol, spaceStack );
					}
					
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
											//new Stack<mysSymbolSpace>( spaceStack ),
											spaceStack.Clone(),
											passedArgs
										)
									);
								} else {
									currentExpression.Insert(
										currentLast,
										matching.Call(
											//new Stack<mysSymbolSpace>( spaceStack ),
											spaceStack.Clone(),
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

					currentLast--;
				}

				break;
			}

			if ( queue.Count > 1 ) {
				return new mysList( queue.Reverse().ToList() );
			} else {
				return queue.Dequeue();
			}
		}
	}

	public enum mysTypes {
		Symbol,
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
					// va = type expected
					// a.Type = type received
					.Zip( arguments, (va, a) => va == a.Type )
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

		public static Stack<T> Clone<T>(this Stack<T> stack) {
			Contract.Requires( stack != null );
			return new Stack<T>( new Stack<T>( stack ) );
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

		//public List<mysToken> Function;
		public mysList Function;

		public mysFunction() {
			Signature = new List<mysTypes>();
			Symbols = new List<mysSymbol>();

			//Function = new List<mysToken>();
			Function = new mysList();
		}

		// might want to move the stack cloning inside here, instead of having
		// to do it every time we call a function outside? gets cleaner that way
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
			return Function.Evaluate( spaceStack );
		}
	}

	public static class mysBuiltins {
		public static mysFunctionGroup Addition;

		static void SetupAddition( mysSymbolSpace global ) {
			Addition = new mysFunctionGroup();

			mysBuiltin addition;

			// int int variant
			addition = new mysBuiltin();
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
			//

			global.Define(
				global.Create( "+" ),
				Addition
			);
		}

		public static void Setup( mysSymbolSpace global ) {
			SetupAddition( global );
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
