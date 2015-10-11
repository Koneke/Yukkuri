using System;

namespace mysharp
{
	// TYPE DUMMIES!
	public class CLR { }
	public static class NUMBER {
		public static double Promote( mysToken n ) {
			if ( n.Type == typeof(int) ) return (double)(int)n.Value;
			if ( n.Type == typeof(long) ) return (double)(long)n.Value;
			if ( n.Type == typeof(float) ) return (double)(float)n.Value;

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

				// hahah xd lets make the type of Type not be Type
				// ty m$
				// in reality, that *might* be a good thing? maybe?
				// for now though we'll just do this hack
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
			bool plainAssignable = a == b;

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
				plainAssignable ||
				anyAssignable ||
				numberAssignable ||
				clrAssignable
			;
		}

		public override string ToString() {
			mysSymbol s = Value as mysSymbol;
			if ( s != null ) return "s";

			//return InternalValue.ToString();

			// verboser
			return string.Format(
				"{0}:{1}",
				Type.ToString(),
				Value.ToString()
			);
		}
	}
}
