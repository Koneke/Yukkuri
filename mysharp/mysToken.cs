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

		protected object InternalValue;

		public mysToken(
			object value,
			mysTypes type
		) {
			InternalValue = value;
			Type = type;
		}

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
		public mysTypes Value {
			get {
				return (mysTypes)InternalValue;
			}
		}

		public mysTypeToken( mysTypes typeValue )
			: base ( typeValue, mysTypes.mysType )
		{
		}

		public override string ToString()
		{
			return $"(typetoken: {Value})";
		}
	}

	public class mysIntegral : mysToken
	{
		public long Value {
			get { return (long)InternalValue; }
		}

		public mysIntegral( long value )
			: base ( value, mysTypes.Integral )
		{
		}

		public override string ToString()
		{
			return $"(int: {Value})";
		}
	}

	public class mysFloating : mysToken
	{
		public double Value {
			get { return (double)InternalValue; }
		}

		public mysFloating( double value )
			: base ( value, mysTypes.Floating )
		{
		}

		public override string ToString()
		{
			return $"(fl: {Value})";
		}
	}

	public class mysString : mysToken
	{
		public string Value {
			get { return (string)InternalValue; }
		}

		public mysString( string value )
			: base ( value, mysTypes.String )
		{
		}

		public override string ToString()
		{
			return $"(str: {Value})";
		}
	}

	public class clrObject : mysToken
	{
		public object Value {
			get { return InternalValue; }
		}

		public clrObject( object value )
			: base ( value, mysTypes.clrObject )
		{
			;
		}

		public override string ToString()
		{
			return $"(clr: {Value})";
		}
	}
}
