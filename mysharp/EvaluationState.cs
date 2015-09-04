﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace mysharp
{
	public class EvaluationMachine {

		// move these to mysSymbol?
		// less args and makes more sense semantically

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

		List<mysToken> tokens;
		Stack<mysSymbolSpace> spaceStack;

		mysSymbol symbolic;
		int current;

		public EvaluationMachine(
			List<mysToken> tokenList,
			Stack<mysSymbolSpace> spaceStack
		) {
			tokens = new List<mysToken>( tokenList );
			this.spaceStack = spaceStack;
		}

		public List<mysToken> Evaluate() {
			// evaluate lists before anything else since they should *always*
			// have prio. they will of course also recursively evaluate lists
			// within themselves.
			EvaluateLists();

			while( CanStep() ) {
				Step();
			}

			return tokens;
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

		public bool CanStep() {
			return current < tokens.Count;
		}

		void preprocessSymbol() {
			symbolic = tokens[ current ] as mysSymbol;

			if ( !tokens[ current ].Quoted ) {
				tokens[ current ] = EvaluateSymbol(
					tokens[ current ] as mysSymbol,
					spaceStack
				);
			} else {
				tokens[ current ].Quoted = false;
			}
		}

		void resolveFunctionGroup() {
			mysFunctionGroup fg = tokens[ current ] as mysFunctionGroup;

			int signatureLength = fg.Variants
				.OrderBy( v => v.SignatureLength )
				.Last()
				.SignatureLength
			;

			mysFunction f = null;
			for ( int j = signatureLength; j >= 0; j-- ) {
				List<mysToken> args = tokens
					.Skip( current + 1 )
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

			tokens.RemoveAt( current );
			tokens.Insert( current, f );
		}

		void handleFunction() {
			mysFunction f = tokens[ current ] as mysFunction;

			mysToken t = f.Call(
				spaceStack, // should be last arg really...
				tokens
					.Skip( current + 1 )
					.Take( f.SignatureLength )
					.ToList()
			);

			tokens.RemoveRange( current, f.SignatureLength + 1 );

			if ( t != null ) {
				tokens.Insert( current, t );
			}
		}

		public void Step() {
			// we use this to keep track of the original symbol (if the current
			// token is one), so we can refer back to the symbol given in the
			// code, even after dereferencing it internally.
			symbolic = null;

			// where is our quote check right now..?

			if ( tokens[ current ].Type == mysTypes.Symbol ) {
				preprocessSymbol();
			}

			if ( tokens[ current ].Type == mysTypes.FunctionGroup ) {
				resolveFunctionGroup();
			}

			switch ( tokens[ current ].Type ) {
				case mysTypes.Function:
					handleFunction();
					break;

				// do we really need this list here..?

				// only quoted symbols end up here, so we ok
				case mysTypes.Symbol:
				// same here, should only be quoted ones
				// (we have already processed nonquoted lists)
				// (although I guess a function could return a nonquoted..?)
				// (we'll have to look into that)
				case mysTypes.List:
				case mysTypes.String:
				case mysTypes.Integral:
				case mysTypes.Floating:
				case mysTypes.mysType:
					break;
				default:
					throw new ArgumentException();
			}

			current++;
		}
	}
}
