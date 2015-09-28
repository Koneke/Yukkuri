using System;
using System.Collections.Generic;

namespace mysharp
{
	// this should more or less all be redone
	// we should probably do something like just have the mystoken class
	// and have it have one variable for its type, and one for the value,
	// really, instead of having all these different classes.
	// unsure if a generic class would work, thinking not (we need to be able
	// to keep these tokens in a single collection still, so they shouldn't
	// really be different types; atleast they need a shared base type or an
	// interface or something).
	// this'll be a bit of a hassle, but it needs to be done at some point
	// to make the clr interactions less awkward.
	// unsure how to handle special types, like symbol and function groups and
	// stuff?
	// might have to/want to keep them as their own typ still, might work well.

	public enum mysTypes {
		// capitals are, for future reference, the more "meta" types
		// probably to change at some point
		NULLTYPE,
		ANY,
		Symbol,
		Integral,
		Floating,
		NUMBER,
		Boolean,
		List,
		String,
		Function,
		FunctionGroup,
		mysType,

		clrObject,
		clrType,
		CLR,
		clrFunction,
		clrFunctionGroup
	}

	public class mysToken
	{
		public mysTypes Type;
		public bool Quoted;

		public object InternalValue;

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

		// whether or not a is assignable from b
		public static bool AssignableFrom(
			mysTypes a,
			mysTypes b
		) {
			bool plainAssignable = a == b;

			bool anyAssignable = a == mysTypes.ANY;

			bool numberAssignable = (
				a == mysTypes.NUMBER &&
				( b == mysTypes.Integral || b == mysTypes.Floating )
			);

			bool clrAssignable = (
				a == mysTypes.CLR &&
				( b == mysTypes.clrObject || b == mysTypes.clrType )
			);

			return
				plainAssignable ||
				anyAssignable ||
				numberAssignable ||
				clrAssignable
			;
		}

		public static mysList PromoteToList(
			mysToken item
		) {
			if ( item.Type == mysTypes.List ) {
				return item as mysList;
			}

			List<mysToken> newlist = new List<mysToken>();
			newlist.Add( item );

			return new mysList( newlist );
		}

		public static bool IsNumber(
			mysToken token
		) {
			return AssignableFrom( mysTypes.NUMBER, token.Type );
		}

		public static mysFloating PromoteToFloat(
			mysToken number
		) {
			if (
				number.Type != mysTypes.Integral &&
				number.Type != mysTypes.Floating )
			{
				throw new ArgumentException();
			}

			if ( number.Type == mysTypes.Integral ) {
				return new mysFloating( (number as mysIntegral).Value );
			}

			return (mysFloating)number;
		}

		public static bool CanSafelyDemoteNumber(
			mysToken number
		) {
			if ( !AssignableFrom( mysTypes.NULLTYPE, number.Type ) ) {
				return false;
			}

			if ( number.Type == mysTypes.Integral ) {
				return true;
			}

			if ( (number as mysFloating).Value % 1 == 0 ) {
				return true;
			}

			return false;
		}

		public static mysIntegral DemoteNumber(
			mysToken number
		) {
			if ( !CanSafelyDemoteNumber( number ) ) {
				throw new ArgumentException();
			}

			if ( number.Type == mysTypes.Integral ) {
				return number as mysIntegral;
			}

			return new mysIntegral( (int)(number as mysFloating).Value );
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

		// UNTESTED, UNSURE ABOUT EXACT BEHAVIOUR
		public bool CanSafelyBeDemoted() {
			return Value % 1 == 0;
		}

		public mysIntegral Demote() {
			return new mysIntegral(
				(int)Math.Round( Value )
			);
		}

		public override string ToString()
		{
			return $"(fl: {Value})";
		}
	}

	public class mysBoolean : mysToken
	{
		public bool Value {
			get { return (bool)InternalValue; }
		}

		public mysBoolean( bool value )
			: base ( value, mysTypes.Boolean )
		{
		}

		public override string ToString()
		{
			return $"(bool: {Value})";
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

	public class clrType : mysToken
	{
		public Type Value {
			get { return (Type)InternalValue; }
		}

		public clrType( Type value )
			: base ( value, mysTypes.clrType )
		{
			;
		}

		public override string ToString()
		{
			return $"(clr-type: {Value})";
		}
	}
}
