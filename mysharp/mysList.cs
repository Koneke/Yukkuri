using System;
using System.Collections.Generic;
using System.Linq;

namespace mysharp
{
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
}
