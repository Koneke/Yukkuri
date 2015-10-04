using System.Collections.Generic;

namespace mysharp.Builtins.Flow
{
	public static class If 
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( typeof(bool) );
			f.Signature.Add( typeof(mysList) );
			f.Signature.Add( typeof(mysList) );

			f.Function = (args, state, sss) => {
				mysToken condition = args[ 0 ];
				mysList positive = args[ 1 ] as mysList;
				mysList negative = args[ 2 ] as mysList;

				EvaluationMachine em;
				if ( (bool)condition.InternalValue ) {
					em = new EvaluationMachine(
						positive.InternalValues,
						state,
						sss
					);
				} else {
					em = new EvaluationMachine(
						negative.InternalValues,
						state,
						sss
					);
				}

				List<mysToken> result = em.Evaluate();

				return result;
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "if", functionGroup, global );
		}
	}

	public static class When
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( typeof(bool) );
			f.Signature.Add( typeof(mysList) );

			f.Function = (args, state, sss) => {
				mysToken condition = args[ 0 ];
				mysList positive = args[ 1 ] as mysList;

				EvaluationMachine em;
				if ( (bool)condition.InternalValue ) {
					em = new EvaluationMachine(
						positive.InternalValues,
						state,
						sss
					);
				} else {
					return null;
				}

				List<mysToken> result = em.Evaluate();

				return result;
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "when", functionGroup, global );
		}
	}
}
