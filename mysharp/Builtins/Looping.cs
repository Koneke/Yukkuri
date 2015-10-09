using System;
using System.Collections.Generic;

namespace mysharp.Builtins.Looping
{
	public static class While
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( typeof(List<mysToken>) );
			f.Signature.Add( typeof(List<mysToken>) );

			f.Function = (args, state, sss) => {
				List<mysToken> conditional = (List<mysToken>)args[ 0 ].InternalValue;
				List<mysToken> body = (List<mysToken>)args[ 1 ].InternalValue;

				List<mysToken> finalReturn = null;

				while ( true ) {
					// might want to move this outside somehow, and/or make
					// em non-destructive, so we can call evaluate several times
					// without making a new em all the time.
					// not really expensive to make new ones though I guess, but
					// it does look a bit clunky.
					EvaluationMachine em = new EvaluationMachine(
						conditional,
						state,
						sss
					);

					mysToken condition = em.Evaluate().Car();

					if ( condition == null ) {
						throw new ArgumentException();
					}

					if ( !(bool)condition.InternalValue ) {
						break;
					}

					em = new EvaluationMachine(
						body,
						state,
						sss
					);

					finalReturn = em.Evaluate();
				}

				// do we actually need this, or should it be null..?
				return finalReturn;
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "while", functionGroup, global );
		}
	}

	public static class For
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( typeof(List<mysToken>) );
			f.Signature.Add( typeof(List<mysToken>) );

			f.Function = (args, state, sss) => {
				List<mysToken> head = (List<mysToken>)args[ 0 ].InternalValue;
				List<mysToken> body = (List<mysToken>)args[ 1 ].InternalValue;

				mysSymbol symbol;
				List<mysToken> collection;

				if ( head.Count != 2 ) {
					throw new ArgumentException();
				}

				EvaluationMachine em;

				symbol = head[ 0 ].InternalValue as mysSymbol;

				if ( head[ 1 ].Type == typeof(mysSymbol) ) {
					collection = 
						(List<mysToken>)
						(head[ 1 ].InternalValue as mysSymbol)
						.Value( sss )
						.InternalValue
					;

				} else {
					collection = (List<mysToken>)head[ 1 ].InternalValue;
				}

				/*em = new EvaluationMachine(
					new List<mysToken>() { new mysToken( collection ) },
					state,
					sss
				);

				collection = (List<mysToken>)em.Evaluate().Car().InternalValue;*/

				if ( symbol == null || collection == null ) {
					throw new ArgumentException();
				}

				for ( int i = 0; i < collection.Count; i++ ) {
					// peek our own internal space
					// def iterator variable to current value
					sss.Peek().Define(
						symbol,
						collection[ i ]
					);

					// execute body with given iterator value
					em = new EvaluationMachine(
						body,
						state,
						sss
					);

					em.Evaluate();
				}

				return null;
			};

			functionGroup.Variants.Add( f );
			mysBuiltin.DefineInGlobal( "for", functionGroup, global );
		}
	}
}
