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

	// TYPE DUMMIES!
	public class CLR { }
	public class NUMBER { }
	public class ANY { }

	public class mysToken
	{
		static Dictionary<Type, mysTypes> autoTypes =
			new Dictionary<System.Type, mysTypes>() {
			{ typeof(int), mysTypes.Integral },
			{ typeof(float), mysTypes.Floating },
			{ typeof(bool), mysTypes.Boolean },
			{ typeof(string), mysTypes.String },
			{ typeof(object), mysTypes.clrObject },
			{ typeof(Type), mysTypes.clrType }
		};

		public mysTypes Type;
		public bool Quoted;

		public Type RealType;
		public object InternalValue;

		public mysToken(
			Type realType,
			object value,
			mysTypes type
		) {
			RealType = realType;
			InternalValue = value;
			Type = type;
		}

		public mysToken(
			object value,
			mysTypes type
		) : this( typeof(ValueType), value, type ) { }

		public mysToken(
			object value
		) : this(
			value.GetType(),
			value,
			autoTypes[ value.GetType() ]
		) { }

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
			Type a,
			Type b
		) {
			bool plainAssignable = a == b;

			bool anyAssignable = a == typeof(ANY);

			bool numberAssignable = (
				a == typeof(NUMBER) &&
				( b == typeof(int) || b == typeof(float) )
			);

			bool clrAssignable = (
				a == typeof(CLR) &&
				( b == typeof(object) || b == typeof(Type) )
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
			return AssignableFrom( typeof(NUMBER), token.RealType );
		}

		public static mysToken PromoteToFloat(
			mysToken number
		) {
			if (
				number.Type != mysTypes.Integral &&
				number.Type != mysTypes.Floating )
			{
				throw new ArgumentException();
			}

			if ( number.Type == mysTypes.Integral ) {
				return new mysToken( (float)number.InternalValue );
			}

			return number;
		}

		public static bool CanSafelyDemoteNumber(
			mysToken number
		) {
			// wtf did this do anyways?
			/*if ( !AssignableFrom( mysTypes.NULLTYPE, number.Type ) ) {
				return false;
			}*/

			if ( !AssignableFrom( typeof(NUMBER), number.RealType ) ) {
				return false;
			}

			if ( number.Type == mysTypes.Integral ) {
				return true;
			}

			if ( (float)number.InternalValue % 1 == 0 ) {
				return true;
			}

			return false;
		}

		public static mysToken DemoteNumber(
			mysToken number
		) {
			if ( !CanSafelyDemoteNumber( number ) ) {
				throw new ArgumentException();
			}

			if ( number.Type == mysTypes.Integral ) {
				return number;
			}

			return new mysToken( (int)number.InternalValue, mysTypes.Integral );
		}

		public override string ToString() {
			return InternalValue.ToString();
		}
	}
}
