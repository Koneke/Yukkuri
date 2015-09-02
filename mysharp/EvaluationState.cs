using System;
using System.Collections.Generic;
using System.Linq;

namespace mysharp
{
	public class EvaluationMachine {
		public List<mysToken> Evaluate(
			List<mysToken> tokens,
			Stack<mysSymbolSpace> spaceStack
		) {
			// while any list
			// eval list
			while ( true ) {
				mysList list = tokens.FirstOrDefault( t =>
					t.Type == mysTypes.List &&
					!t.Quoted
				) as mysList;

				if ( list == null ) {
					break;
				}

				int index = tokens.IndexOf( list );
				tokens.Remove( list );

				tokens.InsertRange(
					index,
					Evaluate(
						list.InternalValues,
						spaceStack
					)
				);
			}

			for ( int i = 0; i < tokens.Count(); i++ ) {
				if ( tokens[ i ].Type == mysTypes.Symbol ) {
					if ( !tokens[ i ].Quoted ) {
						tokens[ i ] = EvaluateSymbol(
							tokens[ i ] as mysSymbol,
							spaceStack
						);
					}
				}

				if ( tokens[ i ].Type == mysTypes.FunctionGroup ) {
					mysFunctionGroup fg = tokens[ i ] as mysFunctionGroup;

					int signatureLength = fg.Variants
						.OrderBy( v => v.SignatureLength )
						.Last()
						.SignatureLength
					;

					mysFunction f = null;
					for ( int j = signatureLength; j >= 0; j-- ) {
						List<mysToken> args = tokens
							.Skip( i + 1 )
							.Take( j )
							.ToList()
						;

						f = fg.Judge( args, spaceStack );

						if ( f != null ) {
							break;
						}
					}

					if ( f == null ) {
						throw new NoSuchSignatureException();
					}

					tokens.RemoveAt( i );
					tokens.Insert( i, f );
				}

				switch ( tokens[ i ].Type ) {
					case mysTypes.Function:
						mysFunction f = tokens[ i ] as mysFunction;

						mysToken t = f.Call(
							spaceStack, // should be last arg really...
							tokens
								.Skip( i + 1 )
								.Take( f.SignatureLength )
								.ToList()
						);

						tokens.RemoveRange( i, f.SignatureLength + 1 );

						if ( t != null ) {
							tokens.Insert( i, t );
						}

						break;

					// only quoted symbols end up here, so we ok
					case mysTypes.Symbol:
					// same here, should only be quoted ones
					case mysTypes.List:
					case mysTypes.Integral:
					case mysTypes.Floating:
					case mysTypes.mysType:
						break;
					default:
						throw new ArgumentException();
				}
			}

			return tokens;
		}

		void step() {
		}

		public static mysTypes EvaluateSymbolType(
			mysSymbol symbol,
			Stack<mysSymbolSpace> spaceStack
		) {
			Stack<mysSymbolSpace> evaluationStack = spaceStack.Clone();

			mysToken temp = new mysSymbol( symbol.ToString() );

			while ( temp.Type == mysTypes.Symbol ) {
				temp = EvaluateSymbol( symbol, evaluationStack );
			}

			return temp.Type;
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

	}

	public class mysPreCall : mysToken {
		public List<mysToken> Bunch;

		public mysPreCall(
			mysList bunch,
			mysTypes type
		) {
			Bunch = new List<mysToken>( bunch.InternalValues );
			Type = type;
		}

		public mysPreCall(
			List<mysToken> bunch,
			mysTypes type
		) {
			//Bunch = new mysList( bunch );
			Bunch = new List<mysToken>( bunch );
			Type = type;
		}
	}

	// todo: bunching, i.e. instead of instantly evaluating functions as we
	//       encounter then, we bunch them with their arguments, and make that
	//       a token. then, when there's not callables left, only bunches and
	//       values, we evaluate bunches in reverse order of encountering them
	//       (or by depth, rather?)
	//       which should be a nice left-to-right evaluation.
	//       with that, stuff like
	//         (=> 'some-func '(:int) '(x) '(+ 3 x)) (some-func 2)
	//       should work as expected, unlike now where some-func will be
	//       used first, then defined...
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

		public static mysTypes EvaluateSymbolType(
			mysSymbol symbol,
			Stack<mysSymbolSpace> spaceStack
		) {
			Stack<mysSymbolSpace> evaluationStack = spaceStack.Clone();

			mysToken temp = new mysSymbol( symbol.ToString() );

			while ( temp.Type == mysTypes.Symbol ) {
				temp = EvaluateSymbol( symbol, evaluationStack );
			}

			return temp.Type;
		} 

		public static mysTypes EvaluateTokenType(
			mysToken t,
			Stack<mysSymbolSpace> spaceStack
		) {
			if ( t.Type == mysTypes.Symbol ) {
				return EvaluateSymbolType( t as mysSymbol, spaceStack );
			} else {
				return t.Type;
			}
		}

		mysToken current;
		int currentIndex;

		Queue<mysToken> queue;
		List<mysToken> currentExpression;
		Stack<mysSymbolSpace> evaluationStack;
		List<mysPreCall> bunchStack;

		public EvaluationState(
			List<mysToken> initial,
			Stack<mysSymbolSpace> spaceStack
		) {
			queue = new Queue<mysToken>();
			currentExpression = new List<mysToken>( initial );
			bunchStack = new List<mysPreCall>();

			evaluationStack = spaceStack;

			currentIndex = currentExpression.Count - 1;
		}

		public mysToken Evaluate() {
			throw new NotImplementedException();
		}

		// true if we should continue afterwards
		void handleQuote() {
			current.Quoted = false;
			queue.Enqueue( current );
			currentIndex--;
		}

		mysToken evaluateFunction(
			mysFunction f,
			List<mysToken> args
		) {
			mysToken returned;

			if ( f is mysBuiltin ) {
				returned =
					(f as mysBuiltin).Call(
					evaluationStack,
					args
				);
			} else {
				returned = f.Call(
					evaluationStack,
					args
				);
			}

			return returned;
		}

		// should be evaluate functiongroup/function *TOKEN*
		/*int evaluateFunctionGroup(
			out mysToken outval
		) {
			mysFunctionGroup fg = current as mysFunctionGroup;
			List<mysToken> passedArgs = queue.Reverse().ToList();

			while ( passedArgs.Count >= 0 ) {
				mysFunction matching = fg.Judge(
					passedArgs,
					spaceStack
				);

				if ( matching != null ) {
					List<mysToken> bunch = new List<mysToken>( passedArgs );
					bunch.Insert( 0, matching );

					mysPreCall pc = new mysPreCall(
						bunch,
						matching.ReturnType
					);

					//bunchStack.Add( pc );

					outval = pc;

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
		}*/

		/*void handleFunctionGroup() {
			mysToken returned;

			int taken = evaluateFunctionGroup(
				out returned
			);

			currentExpression.RemoveRange(
				currentIndex, taken + 1
			);

			// if the bunch returns anything, insert it here
			//if ( returned != null ) {
			if ( returned.Type != mysTypes.NULLTYPE ) {
				currentExpression.Insert(
					currentIndex,
					returned as mysPreCall
				);
			}

			// no matter if the bunch evaluation returns anything or not,
			// add it to the bunchstack.
			bunchStack.Add( returned as mysPreCall );

			currentIndex = currentExpression.Count;
			queue.Clear();
		}*/

		public bool CanStep() {
			return
				currentIndex >= 0 &&
				currentExpression.Count > currentIndex &&
				currentExpression.Any( t => !t.Quoted && (
					t.Type == mysTypes.Symbol ||
					t.Type == mysTypes.Function ||
					t.Type == mysTypes.FunctionGroup ||
					t.Type == mysTypes.List
				))
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

			switch ( EvaluateTokenType( current, evaluationStack ) ) {
				case mysTypes.Function:
					mysFunction f = current as mysFunction;

					List<mysToken> taken = currentExpression
						.Skip( currentIndex)
						.Take( f.SignatureLength + 1 )
						.ToList()
					;

					currentExpression.RemoveRange(
						currentIndex, f.SignatureLength + 1
					);

					mysPreCall prec = new mysPreCall(
						taken,
						f.ReturnType
					);

					if ( f.ReturnType != mysTypes.NULLTYPE ) {
						currentExpression.Insert(
							currentIndex,
							prec
						);
					}

					bunchStack.Add( prec );

					currentIndex = currentExpression.Count;
					queue.Clear();

					break;

				case mysTypes.FunctionGroup:
					//handleFunctionGroup();
					break;

				case mysTypes.List:

					mysPreCall pc = new mysPreCall(
						current as mysList,
						mysTypes.List
					);

					bunchStack.Add( pc );

					currentExpression.Remove( current );
					currentExpression.Insert( currentIndex, pc );

					queue.Enqueue( pc );

					break;

				default:
					queue.Enqueue( current );
					break;
			}

			currentIndex--;
		}
	}
}
