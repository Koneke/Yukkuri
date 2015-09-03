using System;
using System.Collections.Generic;
using System.Linq;

namespace mysharp
{
	public class EvaluationMachine {
		List<mysToken> tokens;
		Stack<mysSymbolSpace> spaceStack;

		public EvaluationMachine(
			List<mysToken> tokenList,
			Stack<mysSymbolSpace> spaceStack
		) {
			tokens = new List<mysToken>( tokenList );
			this.spaceStack = spaceStack;
		}

		void EvaluateLists() {
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

				EvaluationMachine em = new EvaluationMachine(
					list.InternalValues,
					spaceStack
				);

				tokens.InsertRange(
					index,
					em.Evaluate()
				);
			}
		}

		public List<mysToken> Evaluate() {
			// operate on copy so we're non-destructive
			//tokens = new List<mysToken>( tokenList );

			EvaluateLists();

			mysSymbol symbolic = null;

			for ( int i = 0; i < tokens.Count(); i++ ) {
				if ( tokens[ i ].Type == mysTypes.Symbol ) {
					symbolic = tokens[ i ] as mysSymbol;

					if ( !tokens[ i ].Quoted ) {
						tokens[ i ] = EvaluateSymbol(
							tokens[ i ] as mysSymbol,
							spaceStack
						);
					} else {
						tokens[ i ].Quoted = false;
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
						throw new NoSuchSignatureException(
							string.Format(
								"Can't evaluate functiongroup {0}: " +
								"No such signature exists.",
								symbolic != null
									? symbolic.ToString()
									: "(unknown symbol)"
							)
						);
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

					// do we really need this list here..?

					// only quoted symbols end up here, so we ok
					case mysTypes.Symbol:
					// same here, should only be quoted ones
					case mysTypes.List:
					case mysTypes.String:
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

			throw new ArgumentException(
				string.Format(
					"Can't evaluate symbol {0}: Symbol isn't defined.",
					symbol.ToString()
				)
			);
		}
	}
}
