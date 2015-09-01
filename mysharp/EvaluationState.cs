using System;
using System.Collections.Generic;
using System.Linq;

namespace mysharp
{
	public class EvaluationState
	{
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

		mysToken current;
		int currentIndex;

		Queue<mysToken> queue;
		List<mysToken> currentExpression;
		Stack<mysSymbolSpace> evaluationStack;

		//public void Create(
		public EvaluationState(
			List<mysToken> initial,
			Stack<mysSymbolSpace> spaceStack
		) {
			queue = new Queue<mysToken>();
			currentExpression = new List<mysToken>( initial );

			// why do we clone...? should reusing it be fine?
			// even preferable, in fact?
			evaluationStack = spaceStack.Clone();

			currentIndex = currentExpression.Count - 1;
		}

		public mysToken Evaluate() {
			while( CanStep() ) {
				Step();
			}

			if ( queue.Count > 1 ) {
				return new mysList(
					queue.Reverse().ToList()
				);
			} else {
				if ( queue.Count == 0 ) {
					return null;
				}

				return queue.Dequeue();
			}
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

		public bool CanStep() {
			return
				currentIndex >= 0 &&
				currentExpression.Count > currentIndex
			;
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

}
