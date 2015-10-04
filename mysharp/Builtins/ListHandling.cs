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

			f.Signature.Add( typeof(mysList) );

			f.Function = (args, state, sss) => {
				mysList l = new mysList(
					( args[ 0 ] as mysList ).InternalValues,
					true
				);

				l.InternalValues.Reverse();

				return new List<mysToken>() { l };
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

			//f.returnType

			f.Signature.Add( typeof(mysList) );

			f.Function = (args, state, sss) =>
				new List<mysToken>() {
					( args[ 0 ] as mysList )
						.InternalValues
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

			f.Signature.Add( typeof(mysList) );

			f.Function = (args, state, sss) =>
				new List<mysToken>() {
					new mysList(
						( args[ 0 ] as mysList ).InternalValues
							.Skip( 1 )
							.ToList()
					).Quote( args[ 0 ].Quoted )
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
				mysList first = mysToken.PromoteToList( args[ 0 ] );
				mysList second = mysToken.PromoteToList( args[ 1 ] );

				List<mysToken> outList = new List<mysToken>();
				outList.AddRange( first.InternalValues );
				outList.AddRange( second.InternalValues );

				mysList newList = new mysList( outList, true );

				return new List<mysToken>() {
					newList
				};
			};

			functionGroup.Variants.Add( f );

			// make-list version

			f = new mysBuiltin();

			f.Signature.Add( typeof(ANY) );

			f.Function = (args, state, sss) => {
				mysList first = mysToken.PromoteToList( args[ 0 ] );

				return new List<mysToken>() {
					first.Quote()
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

			f.Signature.Add( typeof(mysList) );

			f.Function = (args, state, sss) => {
				mysList first = mysToken.PromoteToList( args[ 0 ] );

				return new List<mysToken>() {
					new mysToken(
						(args[ 0 ] as mysList).InternalValues.Count()
					)
				};
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "len", functionGroup, global );
		}
	}
}
