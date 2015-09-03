﻿namespace mysharp
{
	public enum mysTypes {
		NULLTYPE,
		ANY,
		Symbol,
		Integral,
		Floating,
		List,
		String,
		Function,
		FunctionGroup,
		mysType,
		clrObject
	}

	public class mysToken
	{
		public mysTypes Type;
		public bool Quoted;

		public mysToken Quote() {
			Quoted = true;
			return this;
		}

		// more like set-quote
		public mysToken Quote( bool quote ) {
			Quoted = quote;
			return this;
		}
	}

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

		// move symbol evaluate in here? makes sense
	}

	public class mysTypeToken : mysToken
	{
		public mysTypes TypeValue;

		public mysTypeToken( mysTypes typeValue ) {
			Type = mysTypes.mysType;
			TypeValue = typeValue;
		}

		public override string ToString()
		{
			return $"(typetoken: {TypeValue})";
		}
	}

	public class mysIntegral : mysToken
	{
		public long Value;

		public mysIntegral( long value ) {
			Type = mysTypes.Integral;
			Value = value;
		}

		public override string ToString()
		{
			return $"(int: {Value})";
		}
	}

	public class mysFloating : mysToken
	{
		double Value;

		public mysFloating( double value ) {
			Type = mysTypes.Floating;
			Value = value;
		}

		public override string ToString()
		{
			return $"(fl: {Value})";
		}
	}

	public class mysString : mysToken
	{
		string Value;

		public mysString( string value ) {
			Type = mysTypes.String;
			Value = value;
		}

		public override string ToString()
		{
			return $"(str: {Value})";
		}
	}

	public class clrObject : mysToken
	{
		object Value;

		public clrObject( object value ) {
			Type = mysTypes.clrObject;
			Value = value;
		}

		public override string ToString()
		{
			return $"(clr: {Value})";
		}
	}
}
