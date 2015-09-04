using System;
using System.Collections.Generic;

namespace mysharp
{
	public class mysSymbol : mysToken
	{
		private string stringRepresentation;

		public mysSymbol( string symbolString ) {
			Type = mysTypes.Symbol;
			stringRepresentation = symbolString;
		}

		public override bool Equals(object obj)
		{
			if ( obj == null || obj.GetType() != GetType() )
				return false;

			mysSymbol s = (mysSymbol)obj;

			return s.stringRepresentation == stringRepresentation;
		}

		public override int GetHashCode()
		{
			return stringRepresentation.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format(
				"({1}sym: {0})",
				stringRepresentation,
				Quoted ? "q " : ""
			);
		}

		public mysToken EvaluateSymbol(
			Stack<mysSymbolSpace> spaceStack
		) {
			Stack<mysSymbolSpace> evaluationStack = spaceStack.Clone();

			while ( evaluationStack.Count > 0 ) {
				mysSymbolSpace space = evaluationStack.Pop();

				if ( space.Defined( this ) ) {
					return space.GetValue( this );
				}
			}

			throw new ArgumentException(
				string.Format(
					"Can't evaluate symbol {0}: Symbol isn't defined.",
					ToString()
				)
			);
		}

		public mysTypes EvaluateSymbolType(
			Stack<mysSymbolSpace> spaceStack
		) {
			mysToken temp = new mysSymbol( ToString() );

			while ( temp.Type == mysTypes.Symbol ) {
				// EvaluateSymbol clones the stack by itself
				temp = EvaluateSymbol( spaceStack );
			}

			return temp.Type;
		} 
	}

}
