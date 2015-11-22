using System;

namespace mysharp
{
	// TYPE DUMMIES!
	public class CLR { }
	public static class NUMBER {
		public static double Promote( mysToken n ) {
			if ( n.Type == typeof(int) ) return (int)n.Value;
			if ( n.Type == typeof(long) ) return (long)n.Value;
			if ( n.Type == typeof(float) ) return (float)n.Value;
			if ( n.Type == typeof(double) ) return (double)n.Value;

			throw new ArgumentException();
		}
	}
	public class ANY { }

	public class mysToken
	{
		public bool Quoted;

		public Type Type {
			get {
				if ( Value as mysSymbol != null ) {
					return typeof(mysSymbol);
				}

				Type t = Value.GetType();

				// Type is of type System.RuntimeType,
				// which we are not expecting in other places.
				if ( t.FullName == "System.RuntimeType" ) {
					return typeof(Type);
				}

				return Value.GetType();
			}
		}

		public object Value;

		public mysToken(
			object value
		) {
			Value = value;
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
			bool standardAssignable = a.IsAssignableFrom(b);

			bool plainAssignable = a == b; // maybe not necessary if we have the above?

			bool anyAssignable = a == typeof(ANY);

			bool numberAssignable = (
				a == typeof(NUMBER) &&
				(
					b == typeof(int) ||
					b == typeof(long) ||
					b == typeof(double) ||
					b == typeof(float)
				)
			);

			bool clrAssignable = (
				a == typeof(CLR) &&
				( b == typeof(object) || b == typeof(Type) )
			);

			clrAssignable = a == typeof(CLR);

			return
				standardAssignable ||
				plainAssignable ||
				anyAssignable ||
				numberAssignable ||
				clrAssignable
			;
		}

		public override string ToString() {
			mysSymbol s = Value as mysSymbol;
			if ( s != null ) return "s";

			// verboser
			return string.Format(
				"{0}:{1}",
				Type.ToString(),
				Value.ToString()
			);
		}
	}
}
