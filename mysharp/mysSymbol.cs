using System;
using System.Collections.Generic;

namespace mysharp
{
	public class mysSymbol
	{
		public string StringRepresentation;

		// start using the internal value as thing pointed to?
		public mysSymbol( string symbolString )
		{
			StringRepresentation = symbolString;
		}

		public override bool Equals(object obj)
		{
			if ( obj == null || obj.GetType() != GetType() )
				return false;

			mysSymbol s = (mysSymbol)obj;

			return s.StringRepresentation == StringRepresentation;
		}

		public override int GetHashCode()
		{
			return StringRepresentation.GetHashCode();
		}

		public override string ToString()
		{
			return StringRepresentation;
		}

		public mysSymbolSpace DefinedIn(
			Stack<mysSymbolSpace> spaceStack
		) {
			Stack<mysSymbolSpace> evaluationStack = spaceStack.Clone();

			while ( evaluationStack.Count > 0 ) {
				mysSymbolSpace space = evaluationStack.Pop();

				if ( space.Defined( this ) ) {
					return space;
				}
			}

			return null;
		}

		public mysToken Value(
			Stack<mysSymbolSpace> spaceStack
		) {
			mysSymbolSpace space = DefinedIn( spaceStack );

			if ( space != null ) {
				return space.GetValue( this );
			}

			throw new ArgumentException(
				string.Format(
					"Can't evaluate symbol {0}: Symbol isn't defined.",
					ToString()
				)
			);
		}

		public Type DeepType(
			Stack<mysSymbolSpace> spaceStack
		) {
			mysToken temp = new mysToken( new mysSymbol( ToString() ) );

			while ( temp.Type == typeof(mysSymbol) ) {
				// EvaluateSymbol clones the stack by itself
				temp = Value( spaceStack );
			}

			return temp.Type;
		} 
	}
}
