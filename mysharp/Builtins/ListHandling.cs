using System.Collections.Generic;
using System.Linq;

namespace mysharp.Builtins.ListHandling
{
	public static class Car {
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			//f.returnType

			f.Signature.Add( mysTypes.List );

			f.Function = (args, state, sss) =>
				( args[ 0 ] as mysList ).InternalValues
					.FirstOrDefault()
			;

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "car", functionGroup, global );
		}
	}

	public static class Cdr {
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( mysTypes.List );

			f.Function = (args, state, sss) =>
				new mysList(
					( args[ 0 ] as mysList ).InternalValues
						.Skip( 1 )
						.ToList()
				).Quote( args[ 0 ].Quoted )
			;

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

			f.Signature.Add( mysTypes.ANY );
			f.Signature.Add( mysTypes.ANY );

			f.Function = (args, state, sss) => {
				mysList first = mysToken.PromoteToList( args[ 0 ] );
				mysList second = mysToken.PromoteToList( args[ 1 ] );

				second.InternalValues.InsertRange( 0, first.InternalValues );

				return second.Quote();
			};

			functionGroup.Variants.Add( f );

			// make-list version

			f = new mysBuiltin();

			f.Signature.Add( mysTypes.ANY );

			f.Function = (args, state, sss) => {
				mysList first = mysToken.PromoteToList( args[ 0 ] );

				return first.Quote();
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "cons", functionGroup, global );
		}
	}
}
