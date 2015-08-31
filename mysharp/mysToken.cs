namespace mysharp
{
	public enum mysTypes {
		Symbol,
		Integral,
		Floating,
		List,
		FunctionGroup,
		mysType
	}

	public class mysToken
	{
		public mysTypes Type;
		public bool Quoted;

		public mysToken Quote() {
			Quoted = true;
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
			return stringRepresentation;
		}
	}

	public class mysTypeToken : mysToken
	{
		public mysTypes TypeValue;

		public mysTypeToken( mysTypes typeValue ) {
			Type = mysTypes.mysType;
			TypeValue = typeValue;
		}
	}

	public class mysIntegral : mysToken
	{
		public long Value;

		public mysIntegral( long value ) {
			Type = mysTypes.Integral;
			Value = value;
		}
	}

	public class mysFloating : mysToken
	{
		double Value;

		public mysFloating( double value ) {
			Type = mysTypes.Floating;
			Value = value;
		}
	}
}
