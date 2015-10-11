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
			f.Signature.Add( typeof(List<mysToken>) );
			f.Signature.Add( typeof(List<mysToken>) );

			f.Function = (args, state, sss) => {
				bool condition = (bool)args[ 0 ].Value;
				List<mysToken> positive = (List<mysToken>)args[ 1 ].Value;
				List<mysToken> negative = (List<mysToken>)args[ 2 ].Value;

				EvaluationMachine em;
				if ( condition ) {
					em = new EvaluationMachine(
						positive,
						state,
						sss
					);
				} else {
					em = new EvaluationMachine(
						negative,
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
			f.Signature.Add( typeof(List<mysToken>) );

			f.Function = (args, state, sss) => {
				mysToken condition = args[ 0 ];
				List<mysToken> positive = (List<mysToken>)args[ 1 ].Value;


				EvaluationMachine em;
				if ( (bool)condition.Value ) {
					em = new EvaluationMachine(
						positive,
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
