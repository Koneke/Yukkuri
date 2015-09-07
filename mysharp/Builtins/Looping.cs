using System;

namespace mysharp.Builtins.Looping
{
	public static class While
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( mysTypes.List );
			f.Signature.Add( mysTypes.List );

			f.Function = (args, state, sss) => {
				mysList conditional = args[ 0 ] as mysList;

				mysToken finalReturn = null;

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

					mysBoolean condition = em.Evaluate().Car() as mysBoolean;

					if ( condition == null ) {
						throw new ArgumentException();
					}

					if ( !condition.Value ) {
						break;
					}

					em = new EvaluationMachine(
						( args[ 1 ] as mysList ).InternalValues,
						state,
						sss
					);

					finalReturn = em.Evaluate().Car();
				}

				return finalReturn;
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "while", functionGroup, global );
		}
	}
}

