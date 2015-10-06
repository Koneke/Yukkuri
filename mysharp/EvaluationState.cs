using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace mysharp
{
	public class EvaluationMachine {
		List<mysToken> tokens;
		mysState state;
		Stack<mysSymbolSpace> spaceStack;

		mysSymbol symbolic;
		int current;

		public EvaluationMachine(
			List<mysToken> tokenList,
			mysState state,
			Stack<mysSymbolSpace> spaceStack
		) {
			tokens = new List<mysToken>( tokenList );
			this.state = state;
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
			while ( true && tokens.Count > 0 ) {
				mysList list = tokens.FirstOrDefault( t =>
					t.Type == typeof(mysList) &&
					!t.Quoted
				) as mysList;

				if ( list == null ) {
					break;
				}

				int index = tokens.IndexOf( list );
				tokens.Remove( list );

				EvaluationMachine em = new EvaluationMachine(
					list.InternalValues,
					state,
					spaceStack
				);

				List<mysToken> result = em.Evaluate();

				result.RemoveAll( t => t == null );

				tokens.InsertRange(
					index,
					result
				);
			}
		}

		public bool CanStep() {
			return current < tokens.Count;
		}

		void preprocessSymbol() {
			symbolic = tokens[ current ].InternalValue as mysSymbol;

			if ( !tokens[ current ].Quoted ) {
				tokens[ current ] =
					(tokens[ current ].InternalValue as mysSymbol)
					.Value( spaceStack )
				;
			}
		}

		void resolveFunctionGroup() {
			mysFunctionGroup fg =
				tokens[ current ].InternalValue
				as mysFunctionGroup
			;

			int signatureLength = fg.Variants.Max( v => v.SignatureLength );

			mysFunction f = null;
			for ( int j = signatureLength; j >= 0; j-- ) {
				List<mysToken> args = tokens
					.Skip( current + 1 )
					.Take( j )
					.ToList()
				;

				f = fg.Judge( args, spaceStack );

				if ( f != null ) {
					// escape early if we have a positive match
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
			tokens.Insert( current, new mysToken( f ) );
		}

		void resolveClrFunctionGroup() {
			clrFunctionGroup fg =
				tokens[ current ].InternalValue
				as clrFunctionGroup
			;

			mysToken target = tokens[ current + 1 ];

			while ( target.Type == typeof(mysSymbol) ) {
				target =
					(target.InternalValue as mysSymbol)
					.Value( spaceStack )
				;
			}

			Type targetType;

			if ( target.Type == typeof(Type) ) {
				targetType = (Type)target.InternalValue;
			} else {
				targetType = target.InternalValue.GetType();
			}

			List<MethodInfo> variants = targetType
				.GetMethods()
				.Where( m => m.Name == fg.GroupName )
				.ToList()
			;

			int signatureLength = variants
				.Max( v => v.GetParameters().Length );

			clrFunction f = null;
			for ( int j = signatureLength; j >= 0; j-- ) {
				List<mysToken> args = tokens
					.Skip( current + 2 )
					.Take( j )
					.ToList()
				;

				f = clrFunctionGroup.Judge( variants, args, spaceStack );

				if ( f != null ) {
					// escape early if we have a positive match
					break;
				}
			}

			if ( f == null ) {
				throw new NoSuchSignatureException(
					string.Format(
						"Can't evaluate clrfunctiongroup {0}: " +
						"No such signature exists.",
						symbolic != null
							? symbolic.ToString()
							: "(unknown symbol)"
					)
				);
			}

			tokens.RemoveAt( current );
			tokens.Insert( current, new mysToken( f ) );
		}

		void handleFunction() {
			mysFunction f =
				tokens[ current ].InternalValue
				as mysFunction
			;

			List<mysToken> t = f.Call(
				tokens
					.Skip( current + 1 )
					.Take( f.SignatureLength )
					.ToList(),
				state,
				spaceStack
			);

			tokens.RemoveRange( current, f.SignatureLength + 1 );

			if ( t != null ) {
				tokens.InsertRange( current, t );
			}
		}

		void handleClrFunction() {
			clrFunction f =
				tokens[ current ].InternalValue
				as clrFunction
			;

			mysToken target = tokens[ current + 1 ];

			while ( target.Type == typeof(mysSymbol) ) {
				target =
					(target.InternalValue as mysSymbol)
					.Value( spaceStack )
				;
			}

			List<mysToken> t = f.Call(
				target,
				tokens
					.Skip( current + 2 )
					.Take( f.SignatureLength )
					.ToList(),
				state,
				spaceStack
			);

			tokens.RemoveRange( current, f.SignatureLength + 2 );

			if ( t != null ) {
				tokens.InsertRange( current, t );
			}
		}

		public void Step() {
			// we use this to keep track of the original symbol (if the current
			// token is one), so we can refer back to the symbol given in the
			// code, even after dereferencing it internally.
			symbolic = null;

			// where is our quote check right now..?

			if ( tokens[ current ].Type == typeof(mysSymbol) ) {
				preprocessSymbol();
			}

			if (
				tokens[ current ].Type == typeof(mysFunctionGroup) &&
				!tokens[ current ].Quoted
			) {
				resolveFunctionGroup();
			}

			if (
				tokens[ current ].Type == typeof(clrFunctionGroup) &&
				!tokens[ current ].Quoted
			) {
				resolveClrFunctionGroup();
			}

			if (
				tokens[ current ].Type == typeof(mysFunction) ||
				tokens[ current ].Type == typeof(mysBuiltin)
			) {
				handleFunction();
			}
			else if ( tokens[ current ].Type == typeof(clrFunction ) ) {
				handleClrFunction();
			}

			current++;
		}
	}
}
