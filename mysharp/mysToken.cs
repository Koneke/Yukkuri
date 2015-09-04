namespace mysharp
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
