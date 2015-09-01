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

		class EvaluationState
		{
			mysToken current;
			int currentIndex;

			Queue<mysToken> queue;
			List<mysToken> currentExpression;
			Stack<mysSymbolSpace> evaluationStack;

			public void Create(
				List<mysToken> initial,
				Stack<mysSymbolSpace> spaceStack
			) {
				queue = new Queue<mysToken>();
				currentExpression = new List<mysToken>( initial );

				// why do we clone...? should reusing it be fine?
				// even preferable, in fact?
				evaluationStack = spaceStack.Clone();

				currentIndex = currentExpression.Count;
			}

			// true if we should continue afterwards
			void handleQuote() {
				current.Quoted = false;
				queue.Enqueue( current );
				currentIndex--;
			}

			int evaluateFunctionGroup(
				//Stack<mysSymbolSpace> evaluationStack,
				out mysToken outval
			) {
				mysFunctionGroup fg = current as mysFunctionGroup;
				List<mysToken> passedArgs = queue.Reverse().ToList();

				while ( passedArgs.Count >= 0 ) {
					mysFunction matching = fg.Judge( passedArgs );

					if ( matching != null ) {
						// call function and add our result back into
						// the expression
						mysToken returned;
						if ( matching is mysBuiltin ) {
							returned =
								(matching as mysBuiltin).Call(
								evaluationStack,
								passedArgs
							);
						} else {
							returned = matching.Call(
								evaluationStack,
								passedArgs
							);
						}

						outval = returned;

						return passedArgs.Count;
					} else {
						if ( passedArgs.Count > 0 ) {
							// remove last, try again.
							passedArgs.RemoveAt( passedArgs.Count - 1 );
						} else {
							throw new NoSuchSignatureException();
						}
					}
				}

				throw new NoSuchSignatureException();
			}

			void handleFunctionGroup() {
				mysToken returned;

				int taken = evaluateFunctionGroup(
					out returned
				);

				currentExpression.RemoveRange(
					currentIndex, taken + 1
				);

				if ( returned != null ) {
					currentExpression.Insert(
						currentIndex,
						returned
					);
				}

				currentIndex = currentExpression.Count;
				queue.Clear();
			}

			public void Step() {
				current = currentExpression.ElementAt( currentIndex );

				if ( !current.Quoted ) {
					// deref
					while ( current.Type == mysTypes.Symbol ) {
						current = EvaluateSymbol(
							current as mysSymbol,
							evaluationStack
						);
					}
				} else {
					handleQuote();
					return;
				}

				switch ( current.Type ) {
					case mysTypes.FunctionGroup:
						handleFunctionGroup();
						break;

					case mysTypes.List:
						mysToken returned = ( current as mysList )
							.Evaluate( evaluationStack );

						if ( returned != null ) {
							queue.Enqueue( returned );
						}
						break;

					default:
						queue.Enqueue( current );
						break;
				}

				currentIndex--;
			}
		}

		int evaluateFunctionGroup(
			mysFunctionGroup fg,
			List<mysToken> passedArgs,
			Stack<mysSymbolSpace> evaluationStack,
			out mysToken outval
		) {
			while ( passedArgs.Count >= 0 ) {
				mysFunction matching = fg.Judge( passedArgs );

				if ( matching != null ) {
					// call function and add our result back into
					// the expression
					mysToken returned;
					if ( matching is mysBuiltin ) {
						returned =
							(matching as mysBuiltin).Call(
							evaluationStack,
							passedArgs
						);
					} else {
						returned = matching.Call(
							evaluationStack,
							passedArgs
						);
					}

					outval = returned;

					return passedArgs.Count;
				} else {
					if ( passedArgs.Count > 0 ) {
						// remove last, try again.
						passedArgs.RemoveAt( passedArgs.Count - 1 );
					} else {
						throw new NoSuchSignatureException();
					}
				}
			}

			throw new NoSuchSignatureException();
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

						mysToken returned;

						int taken = evaluateFunctionGroup(
							fg,
							passedArgs,
							evaluationStack,
							out returned
						);

						currentExpression.RemoveRange(
							currentLast, taken + 1
						);

						if ( returned != null ) {
							//queue.Enqueue( returned );
							currentExpression.Insert(
								currentLast,
								returned
							);
						}

						currentLast = currentExpression.Count;
						queue.Clear();
					} else {
						if ( last is mysList ) {
							mysToken returned = 
								( last as mysList )
								.Evaluate( spaceStack );

							if ( returned != null ) {
								queue.Enqueue( returned );
							}
						} else {
							queue.Enqueue( last );
						}
					}
					#endregion fg

					currentLast--;
				}

				break;
			}

			if ( queue.Count > 1 ) {
				return new mysList( queue.Reverse().ToList() );
			} else {
				if ( queue.Count == 0 ) return null;

				return queue.Dequeue();
			}
		}
	}
}
