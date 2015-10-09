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

	// TYPE DUMMIES!
	public class CLR { }
	public class NUMBER { }
	public class ANY { }

	public class mysToken
	{
		public bool Quoted;

		public Type Type {
			get {
				if ( InternalValue as mysSymbol != null ) {
					return typeof(mysSymbol);
				}

				Type t = InternalValue.GetType();

				// hahah xd lets make the type of Type not be Type
				// ty m$
				// in reality, that *might* be a good thing? maybe?
				// for now though we'll just do this hack
				if ( t.FullName == "System.RuntimeType" ) {
					return typeof(Type);
				}

				return InternalValue.GetType();
			}
		}
		public object InternalValue;

		protected mysToken() {
		}

		public mysToken(
			object value
		) {
			InternalValue = value;
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

			clrAssignable = a == typeof(CLR);

			return
				plainAssignable ||
				anyAssignable ||
				numberAssignable ||
				clrAssignable
			;
		}

		public static bool IsNumber(
			mysToken token
		) {
			return AssignableFrom( typeof(NUMBER), token.Type );
		}

		public static mysToken PromoteToFloat(
			mysToken number
		) {
			if (
				number.Type != typeof(int) &&
				number.Type != typeof(float)
			) {
				throw new ArgumentException();
			}

			if ( number.Type == typeof(int) ) {
				return new mysToken(
					(float)(int)number.InternalValue
				);
			}

			return number;
		}

		public static bool CanSafelyDemoteNumber(
			mysToken number
		) {
			if ( !AssignableFrom( typeof(NUMBER), number.Type ) ) {
				return false;
			}

			if ( number.Type == typeof(int) ) {
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

			if ( number.Type == typeof(int) ) {
				return number;
			}

			return new mysToken( (int)number.InternalValue );
		}

		public override string ToString() {
			mysSymbol s = InternalValue as mysSymbol;
			if ( s != null ) return "s";

			//return InternalValue.ToString();

			// verboser
			return string.Format(
				"{0}:{1}",
				Type.ToString(),
				InternalValue.ToString()
			);
		}
	}
}
