using System;
using System.Collections.Generic;
using System.Linq;

namespace mysharp
{
	public class mysPreCall : mysToken {
		public List<mysToken> Bunch;
		//public mysList Bunch;

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

		public mysToken EvaluateBunch( mysPreCall bunch ) {
			EvaluationState bes = new EvaluationState(
				bunch.Bunch,
				evaluationStack
			);

			bes.PreProcess();
			return bes.Evaluate();

			/*EvaluationState bes = new EvaluationState(
				bunch.Bunch,
				evaluationStack
			);
			var a = bes.PreProcess();

			if ( !(a is mysList) ) {
				return a;
			}*/

			//mysToken evaluated = bes.Evaluate();

			List<mysToken> expression = new List<mysToken>(
				bunch.Bunch
			);

			List<mysToken> output = new List<mysToken>();

			// generate sub bunches here???????

			// notice, l to r
			foreach( mysToken t in expression ) {
			//foreach( mysToken t in (a as mysList).InternalValues ) {
				if ( t is mysPreCall ) {
					mysPreCall pc = t as mysPreCall;

					// if we evaluate the bunch, make sure to remove it
					// from the stack so it doesn't get evaluated again by the
					// higher level.
					bunchStack.Remove( pc );

					mysToken returned = EvaluateBunch( pc );

					if ( returned != null ) {
						output.Add( returned );
					}
				} else {
					output.Add( t );
				}
			}

			// update the current expression to post-bunch state, create a new
			// output for the next phase of evaluation.
			expression = output;
			output = new List<mysToken>();

			foreach( mysToken t in expression ) {
				// okay, so this should never possibly be anything taking any
				// kind of at this point undetermined number of arguments,
				// E V E R.
				// I guess this could be problematic if we could return
				// function groups from functions...? maybe? or that might
				// actually already work.
				// either way, it might be a bit picky here.
				mysToken token = t;

				// apparently, we have had a function group slip through to
				// this point.
				while ( token.Type == mysTypes.Symbol ) {
					token = EvaluateSymbol( t as mysSymbol, evaluationStack );
				}

				if ( token is mysFunction ) {
					output.Add(
						evaluateFunction(
							t as mysFunction,
							expression
								.Skip( 1 )
								.Take( expression.Count - 1 )
								.ToList()
						)
					);
					break;
				}

				var a = 0;
			}

			if ( output.Count < 2 ) {
				return output.FirstOrDefault();
			} else {
				return new mysList( output );
			}
		}

		public mysToken PreProcess() {
			// move the 'any processable check' out
			/*while( CanStep() ) {
				if ( currentExpression.Any(
					t => !t.Quoted && (
						t.Type == mysTypes.Symbol ||
						t.Type == mysTypes.Function ||
						t.Type == mysTypes.FunctionGroup ||
						t.Type == mysTypes.List
				))) {
					Step();
				} else {
					break;
				}
			}*/

			while ( CanStep() ) {
				Step();
			}

			// everything should be bunched and nice by now, yes?
			if ( currentExpression.Count < 2 ) {
				return currentExpression.FirstOrDefault();
			} else {
				return new mysList( currentExpression );
			}
		}

		public mysToken Evaluate() {
			//PreProcess();

			List<mysToken> outExpression =
				new List<mysToken>( currentExpression );

			// bunch evaluation
			var a = 0;
			// we want to evaluate it lifo
			bunchStack.Reverse();
			while ( bunchStack.Count > 0 ) {
				mysPreCall bunch = bunchStack.First();

				mysToken result = EvaluateBunch( bunch );
				outExpression.Remove( bunch );
				bunchStack.Remove( bunch );

				if ( result != null ) {
					outExpression.Add( result );
				}
			}

			List<mysToken> output = new List<mysToken>();

			foreach( mysToken t in outExpression ) {
				// okay, so this should never possibly be anything taking any
				// kind of at this point undetermined number of arguments,
				// E V E R.
				// I guess this could be problematic if we could return
				// function groups from functions...? maybe? or that might
				// actually already work.
				// either way, it might be a bit picky here.
				mysToken token = t;

				// apparently, we have had a function group slip through to
				// this point.
				while ( !token.Quoted && token.Type == mysTypes.Symbol ) {
					token = EvaluateSymbol( t as mysSymbol, evaluationStack );
				}

				if ( token is mysFunction ) {
					output.Add(
						evaluateFunction(
							t as mysFunction,
							outExpression
								.Skip( 1 )
								.Take( outExpression.Count - 1 )
								.ToList()
						)
					);
					break;
				}

				output.Add( t );

				var d = 0;
			}

			if ( output.Count < 2 ) {
				return output.FirstOrDefault();
			} else {
				return new mysList( output );
			}

			if ( outExpression.Count < 2 ) {
				return outExpression.FirstOrDefault();
			} else {
				return new mysList( outExpression );
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
		int evaluateFunctionGroup(
			out mysToken outval
		) {
			mysFunctionGroup fg = current as mysFunctionGroup;
			List<mysToken> passedArgs = queue.Reverse().ToList();

			while ( passedArgs.Count >= 0 ) {
				mysFunction matching = fg.Judge( passedArgs );

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
		}

		void handleFunctionGroup() {
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
		}

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

			/*mysTypes type = current.Type;

			if ( type == mysTypes.Symbol ) {
				EvaluateSymbolType(
					current as mysSymbol,
					evaluationStack
				);
			}*/

			//switch ( current.Type ) {
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
					handleFunctionGroup();
					break;

				case mysTypes.List:
					// this wont run anything
					// so it can-
					// except it fucking will though?
					// or, well no? beca
					// fuck, yes it will.
					// because it's not only going to generate
					// bunches, it's going to evaluate them too.

					//mysToken evaluatedList = 
						//(current as mysList)
							//.Evaluate( evaluationStack ) as mysList;

					/*
					
					EvaluationState listes = new EvaluationState(
						(current as mysList).InternalValues,
						evaluationStack
					);

					mysToken preprocessed = listes.PreProcess();

					queue.Enqueue( preprocessed );
					
					*/

					mysPreCall pc = new mysPreCall(
						current as mysList,
						mysTypes.List
					);

					bunchStack.Add( pc );

					/*mysToken evaluatedList = 
						(current as mysList)
							.Evaluate( evaluationStack ) as mysList;
					*/

					/*
					mysPreCall bunch = new mysPreCall(
						//(current as mysList),
						evaluatedList,
						mysTypes.List
					);
					queue.Enqueue( bunch );
					*/

					currentExpression.Remove( current );
					//currentExpression.Insert( currentIndex, bunch );
					//currentExpression.Insert( currentIndex, evaluatedList );
					//currentExpression.Insert( currentIndex, preprocessed );
					currentExpression.Insert( currentIndex, pc );

					queue.Enqueue( pc );

					break;

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
