using System.Collections.Generic;

namespace mysharp
{
	public class mysList : mysToken
	{
		public List<mysToken> InternalValues;

		public mysList( bool quoted = false )
			: this( new List<mysToken>(), quoted ) {
		}

		public mysList( List<mysToken> list, bool quoted = false ) {
			Type = mysTypes.List;
			Quoted = quoted;
			InternalValues = new List<mysToken>( list );
		}

		public mysToken Evaluate(
			Stack<mysSymbolSpace> spaceStack
		) {
			EvaluationState es = new EvaluationState(
				InternalValues,
				spaceStack
			);

			return es.Evaluate();
		}
	}
}
