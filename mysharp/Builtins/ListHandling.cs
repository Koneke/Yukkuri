using System.Linq;
using System.Collections.Generic;

namespace mysharp.Builtins.ListHandling
{
	public static class Reverse {
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			//f.returnType

			f.Signature.Add( typeof(List<mysToken>) );

			f.Function = (args, state, sss) => {
				List<mysToken> l = ((List<mysToken>)args[ 0 ].InternalValue);

				l.Reverse();

				return new List<mysToken>() { new mysToken( l ) };
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "reverse", functionGroup, global );
		}
	}

	public static class Car {
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( typeof(List<mysToken>) );

			f.Function = (args, state, sss) =>
				new List<mysToken>() {
					((List<mysToken>)args[ 0 ].InternalValue)
						.FirstOrDefault()
				};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "car", functionGroup, global );
		}
	}

	public static class Cdr {
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( typeof(List<mysToken>) );

			f.Function = (args, state, sss) => {
				List<mysToken> l =
					((List<mysToken>)args[ 0 ].InternalValue)
					.Skip( 1 )
					.ToList()
				;

				return new List<mysToken>() {
					new mysToken( l ).Quote()
				};
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "cdr", functionGroup, global );
		}
	}

	public static class Cons {
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();
			mysBuiltin f;

			// actual cons version

			f = new mysBuiltin();

			f.Signature.Add( typeof(ANY) );
			f.Signature.Add( typeof(ANY) );

			f.Function = (args, state, sss) => {
				List<mysToken> outList = new List<mysToken>();

				for ( int i = 0; i < 2; i++ ) {
					if ( args[ i ].Type == typeof(List<mysToken>) ) {
						outList.AddRange( (List<mysToken>)args[ i ].InternalValue );
					} else {
						outList.Add( args[ i ] );
					}
				}

				return new List<mysToken>() {
					new mysToken( outList )
				};
			};

			functionGroup.Variants.Add( f );

			// make-list version

			f = new mysBuiltin();

			f.Signature.Add( typeof(ANY) );

			f.Function = (args, state, sss) => {
				return new List<mysToken>() {
					new mysToken( new List<mysToken>() { args[ 0 ] } )
				};
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "cons", functionGroup, global );
		}
	}

	public static class Len {
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();
			mysBuiltin f;

			f = new mysBuiltin();

			f.Signature.Add( typeof(List<>) );

			f.Function = (args, state, sss) => {
				return new List<mysToken>() {
					new mysToken(
						((List<object>)args[ 0 ].InternalValue).Count()
					)
				};
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "len", functionGroup, global );
		}
	}
}
