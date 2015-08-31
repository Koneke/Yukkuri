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
			repl.TestFunction();
		}
	}

	public class mysREPL
	{
		public mysSymbolSpace Global;
		public Dictionary<string, mysSymbolSpace> nameSpaces;
		public Stack<mysSymbolSpace> spaceStack;

		public mysREPL() {
			nameSpaces = new Dictionary<string, mysSymbolSpace>();

			Global = new mysSymbolSpace();
			nameSpaces.Add( "global" , Global );

			spaceStack = new Stack<mysSymbolSpace>();
			spaceStack.Push( Global );

			mysBuiltins.Setup( Global );
		}

		public void TestFunction() {
			// test stuff below

			List<mysToken> testExpression = new List<mysToken>();

			mysFunctionGroup fg = new mysFunctionGroup();
			mysFunction f = new mysFunction();

			f.Symbols.Add( new mysSymbol( "x" ) );
			f.Signature.Add( mysTypes.Integral );
			f.Symbols.Add( new mysSymbol( "y" ) );
			f.Signature.Add( mysTypes.Integral );

			f.Function.InternalValues.Add(
				mysSymbolSpace.GetAndEvaluateSymbol( "+", spaceStack )
			);

			f.Function.InternalValues.Add( new mysSymbol( "x" ) );
			f.Function.InternalValues.Add( new mysSymbol( "y" ) );

			fg.Variants.Add( f );

			testExpression.Add( fg );
			testExpression.Add( new mysIntegral( 2 ) );
			testExpression.Add( new mysIntegral( 3 ) );

			mysList expression = new mysList( testExpression );
			mysToken result = expression.Evaluate( spaceStack );

			var a = 0;

			// lambda test

			testExpression = new List<mysToken>();

			testExpression.Add(
				mysSymbolSpace.GetAndEvaluateSymbol( "=>", spaceStack )
			);

			testExpression.Add(
				new mysSymbol( "some-func" ).Quote()
			);

			testExpression.Add( new mysList( new List<mysToken> {
				new mysTypeToken( mysTypes.Integral )
			} ).Quote() );

			testExpression.Add( new mysList( new List<mysToken> {
				new mysSymbol( "x" )
			} ).Quote() );

			fg = mysSymbolSpace.GetAndEvaluateSymbol( "+", spaceStack )
				as mysFunctionGroup;

			testExpression.Add( new mysList( new List<mysToken> {
				fg,
				new mysSymbol( "x" ),
				new mysIntegral( 3 )
			} ).Quote() );

			expression = new mysList( testExpression );
			result = expression.Evaluate( spaceStack );

			a = 0;

			// lambda result test

			testExpression = new List<mysToken>();

			testExpression.Add(
				mysSymbolSpace.GetAndEvaluateSymbol( "some-func", spaceStack )
			);
			testExpression.Add( new mysIntegral( 2 ) );

			expression = new mysList( testExpression );
			result = expression.Evaluate( spaceStack );

			a = 0;
		}
	}

	public class mysSymbolSpace
	{
		public static mysToken GetAndEvaluateSymbol( 
			string symbolString,
			Stack<mysSymbolSpace> spaceStack
		) {
			return EvaluateSymbol(
				GetSymbol( symbolString, spaceStack ),
				spaceStack
			);
		}

		public static mysToken EvaluateSymbol(
			mysSymbol symbol,
			Stack<mysSymbolSpace> spaceStack
		) {
			Stack<mysSymbolSpace> evaluationStack = spaceStack.Clone();

			while ( evaluationStack.Count > 0 ) {
				mysSymbolSpace space = evaluationStack.Pop();
				if ( space.Defined( symbol ) ) {
					return space.GetValue( symbol );
				}
			}

			throw new ArgumentException( "Symbol isn't defined." );
		}

		public static mysSymbol GetSymbol(
			string symbolString,
			Stack<mysSymbolSpace> spaceStack
		) {
			Stack<mysSymbolSpace> evaluationStack = spaceStack.Clone();

			while ( evaluationStack.Count > 0 ) {
				mysSymbolSpace space = evaluationStack.Pop();

				mysSymbol symbol = space.Values.Keys
					.FirstOrDefault( s => s.ToString() == symbolString );

				if ( symbol != null ) {
					return symbol;
				}

				//if ( space.Exists( symbolString ) ) {
					//return space.Get( symbolString );
				//}
			}

			throw new ArgumentException( "Symbol doesn't exist." );
		}

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

		public void Undefine( mysSymbol symbol ) {
			Values.Remove( symbol );
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

	public class mysToken
	{
		public mysTypes Type;
		public bool Quoted;

		public mysToken Quote() {
			Quoted = true;
			return this;
		}
	}

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

		public override string ToString()
		{
			return stringRepresentation;
		}
	}

	public class mysList : mysToken
	{
		public List<mysToken> InternalValues;

		public mysList( bool quoted = false )
			: this( new List<mysToken>(), quoted ) {
		}

		public mysList( List<mysToken> list, bool quoted = false ) {
			Type = mysTypes.List;
			Quoted = quoted;
			InternalValues = new List<mysToken>( list );
		}

		public static mysToken EvaluateSymbol(
			mysSymbol symbol,
			Stack<mysSymbolSpace> spaceStack
		) {
			Stack<mysSymbolSpace> evaluationStack = spaceStack.Clone();

			while ( evaluationStack.Count > 0 ) {
				mysSymbolSpace space = evaluationStack.Pop();
				if ( space.Defined( symbol ) ) {
					return space.GetValue( symbol );
				}
			}

			throw new ArgumentException( "Symbol isn't defined." );
		}

		public mysToken Evaluate(
			Stack<mysSymbolSpace> spaceStack
		) {
			// do we need the special list case here..? I guess we do?
			if ( Quoted ) {
				Quoted = false;
				return this;
			}

			Queue<mysToken> queue = new Queue<mysToken>();
			List<mysToken> currentExpression =
				new List<mysToken>( InternalValues );

			Stack<mysSymbolSpace> evaluationStack = spaceStack.Clone();

			while ( true ) {
				mysToken last;
				int currentLast = currentExpression.Count - 1;

				while (
					currentLast >= 0 &&
					currentExpression.Count > currentLast
				) {
					last = currentExpression.ElementAt( currentLast );

					mysTypes deepType = last.Type;
					mysToken deepToken = last;

					if ( !last.Quoted ) {
						//while ( deepToken.Type == mysTypes.Symbol ) {
						while ( last.Type == mysTypes.Symbol ) {
							//deepToken = EvaluateSymbol(
							last = EvaluateSymbol(
								//deepToken as mysSymbol,
								last as mysSymbol,
								evaluationStack
							);
							deepType = deepToken.Type;
						}
					} else {
						// unquote, remain a symbol or what the fuck ever we
						// were.
						last.Quoted = false;
						queue.Enqueue( last );
						currentLast--;
						continue;
					}

					#region fg
					//if ( deepType == mysTypes.FunctionGroup ) {
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
											//evaluationStack.Clone(),
											evaluationStack,
											passedArgs
										)
									);
								} else {
									currentExpression.Insert(
										currentLast,
										matching.Call(
											//new Stack<mysSymbolSpace>( spaceStack ),
											//evaluationStack.Clone(),
											evaluationStack,
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
					#endregion fg

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
		FunctionGroup,
		mysType
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

			Function = new mysList();
		}

		// might want to move the stack cloning inside here, instead of having
		// to do it every time we call a function outside? gets cleaner that way
		public virtual mysToken Call(
			Stack<mysSymbolSpace> spaceStack,
			List<mysToken> arguments
		) {
			///Stack<mysSymbolSpace> evaluationStack = spaceStack.Clone();

			// future, cache somehow
			mysSymbolSpace internalSpace = new mysSymbolSpace();

			Symbols.DoPaired(
				arguments,
				(s, a) => internalSpace.Define( s, a )
			);

			//evaluationStack.Push( internalSpace );
			spaceStack.Push( internalSpace );

			mysToken result = Function.Evaluate( spaceStack );

			spaceStack.Pop();

			return result;

			// evaluate
			//return Function.Evaluate( evaluationStack );
		}
	}

	public static class mysBuiltins {
		static void SetupLambda( mysSymbolSpace global ) {
			mysFunctionGroup lambda = new mysFunctionGroup();
			mysBuiltin lambdaVariant = new mysBuiltin();

			lambdaVariant = new mysBuiltin();
			lambdaVariant.Signature.Add( mysTypes.Symbol );
			lambdaVariant.Signature.Add( mysTypes.List );
			lambdaVariant.Signature.Add( mysTypes.List );
			lambdaVariant.Signature.Add( mysTypes.List );

			lambdaVariant.Function = (args, sss) => {
				mysSymbolSpace ss = sss.Peek();

				mysSymbol symbol = args[ 0 ] as mysSymbol;
				mysList types = args[ 1 ] as mysList;
				mysList symbols = args[ 2 ] as mysList;
				mysList body = args[ 3 ] as mysList;

				// argument checking
				if (
					symbol == null || types == null ||
					symbols == null || body == null
				) {
					throw new ArgumentException();
				}

				if (
					types.InternalValues.Count() !=
					symbols.InternalValues.Count()
				) {
					throw new ArgumentException();
				}

				if (
					types.InternalValues.Count <= 0 ||
					types.InternalValues
						.Any(t => (t as mysToken).Type != mysTypes.mysType)
				) {
					throw new ArgumentException();
				}

				if (
					symbols.InternalValues.Count <= 0 ||
					symbols.InternalValues
						.Any(t => (t as mysToken).Type != mysTypes.Symbol)
				) {
					throw new ArgumentException();
				}

				// end argument checking

				// define function variant
				mysFunction f = new mysFunction();
				// these two should probably be joined at some point
				foreach ( mysToken t in types.InternalValues ) {
					f.Signature.Add( ( t as mysTypeToken ).TypeValue );
				}
				foreach ( mysToken t in symbols.InternalValues ) {
					f.Symbols.Add( t as mysSymbol  );
				}

				f.Function = body;
				// end define function variant

				mysFunctionGroup fg = null;

				// if symbol defined and of wrong type, undef it
				if (
					ss.Defined( symbol ) &&
					ss.GetValue( symbol ).Type != mysTypes.FunctionGroup
				) {
					// we could just overwrite it with define,
					// but I'd rather be entirely sure that we delete
					// the old value beforehand.
					ss.Undefine( symbol );
				}

				// if we're defined at this point, we know it's a function group
				if  ( ss.Defined( symbol ) ) {
					fg = ss.GetValue( symbol ) as mysFunctionGroup;
				} else {
					// create 
					fg = new mysFunctionGroup();
					ss.Define( symbol, fg );
				}

				fg.Variants.Add( f );

				// since we return our function group, unless quoted
				// we'll automatically evaluate it.
				// this is probably a good reason for allowing null returns.
				return fg.Quote();
			};

			lambda.Variants.Add( lambdaVariant );

			global.Define(
				global.Create( "=>" ),
				lambda
			);
		}

		static void SetupAddition( mysSymbolSpace global ) {
			mysFunctionGroup addition = new mysFunctionGroup();
			mysBuiltin additionVariant;

			// int int variant
			additionVariant = new mysBuiltin();
			additionVariant.Signature.Add( mysTypes.Integral );
			additionVariant.Signature.Add( mysTypes.Integral );

			additionVariant.Function =
				new Func<List<mysToken>, Stack<mysSymbolSpace>, mysToken>(
				(args, sss) =>
					new mysIntegral(
						(args[ 0 ] as mysIntegral).Value +
						(args[ 1 ] as mysIntegral).Value
					)
			);

			addition.Variants.Add( additionVariant );
			//

			global.Define(
				global.Create( "+" ),
				addition
			);
		}

		public static void Setup( mysSymbolSpace global ) {
			SetupAddition( global );
			SetupLambda( global );
		}
	}

	public class mysBuiltin : mysFunction {
		public new Func<
			List<mysToken>,
			Stack<mysSymbolSpace>,
			mysToken
		> Function;

		// not sure we need to override? but I'm not chancing
		public override mysToken Call(
			Stack<mysSymbolSpace> spaceStack,
			List<mysToken> arguments
		) {
			return Function( arguments, spaceStack );
		}
	}

	public class mysTypeToken : mysToken
	{
		public mysTypes TypeValue;

		public mysTypeToken( mysTypes typeValue ) {
			Type = mysTypes.mysType;
			TypeValue = typeValue;
		}
	}

	public class mysIntegral : mysToken
	{
		public long Value;

		public mysIntegral( long value ) {
			Type = mysTypes.Integral;
			Value = value;
		}
	}

	public class mysFloating : mysToken
	{
		double Value;

		public mysFloating( double value ) {
			Type = mysTypes.Floating;
			Value = value;
		}
	}
}
