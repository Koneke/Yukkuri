﻿using System;
using System.Collections.Generic;

namespace mysharp
{
	public class mysSymbol : mysToken
	{
		public string StringRepresentation;

		public Type Type;

		public mysSymbol( string symbolString )
			// start using the internal value as thing pointed to?
			: base ( typeof(mysSymbol), (object)null )
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
			return string.Format(
				"({1}sym: {0})",
				StringRepresentation,
				Quoted ? "q " : ""
			);
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
			mysToken temp = new mysSymbol( ToString() );

			while ( temp.RealType == typeof(mysSymbol) ) {
				// EvaluateSymbol clones the stack by itself
				temp = Value( spaceStack );
			}

			return temp.RealType;
		} 
	}
}
