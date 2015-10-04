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

			f.Signature.Add( typeof(mysList) );
			f.Signature.Add( typeof(mysList) );

			f.Function = (args, state, sss) => {
				mysList conditional = args[ 0 ] as mysList;

				List<mysToken> finalReturn = null;

				while ( true ) {
					// might want to move this outside somehow, and/or make
					// em non-destructive, so we can call evaluate several times
					// without making a new em all the time.
					// not really expensive to make new ones though I guess, but
					// it does look a bit clunky.
					EvaluationMachine em = new EvaluationMachine(
						conditional.InternalValues,
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
						( args[ 1 ] as mysList ).InternalValues,
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

			f.Signature.Add( typeof(mysList) );
			f.Signature.Add( typeof(mysList) );

			f.Function = (args, state, sss) => {
				mysList head = args[ 0 ] as mysList;
				mysList body = args[ 1 ] as mysList;

				mysSymbol symbol;
				mysList collection;

				if ( head.InternalValues.Count != 2 ) {
					throw new ArgumentException();
				}

				EvaluationMachine em;

				symbol = head.InternalValues[ 0 ] as mysSymbol;
				if ( head.InternalValues[ 1 ].Type == mysTypes.Symbol ) {
					collection = ( head.InternalValues[ 1 ] as mysSymbol )
						.Value( sss ) as mysList;
				} else {
					collection = head.InternalValues[ 1 ] as mysList;
				}

				em = new EvaluationMachine(
					new List<mysToken>() { collection },
					state,
					sss
				);

				collection = em.Evaluate().Car() as mysList;

				if ( symbol == null || collection == null ) {
					throw new ArgumentException();
				}

				for ( int i = 0; i < collection.InternalValues.Count; i++ ) {
					sss.Peek().Define(
						symbol,
						collection.InternalValues[ i ]
					);

					em = new EvaluationMachine(
						body.InternalValues,
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
