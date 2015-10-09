using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System;

namespace mysharp
{
	public class mysFunctionGroup
	{
		// lh: A function group is a collection of functions assigned the same
		//     symbol, but with different signatures.

		public List<mysFunction> Variants;

		public mysFunctionGroup()
		{
			Variants = new List<mysFunction>();
		}

		// lh: returns a matching function, or null if we didn't like the input.
		public mysFunction Judge(
			List<mysToken> arguments,
			Stack<mysSymbolSpace> spaceStack
		) {
			List<mysFunction> variants = new List<mysFunction>( Variants );

			variants.RemoveAll(
				v => !judgeVariant( v, arguments, spaceStack )
			);

			if ( variants.Count == 0 ) {
				return null;
			} else if ( variants.Count != 1 ) {
				throw new SignatureAmbiguityException();
			}

			return variants[ 0 ];
		}

		bool judgeVariant(
			mysFunction variant,
			List<mysToken> arguments,
			Stack<mysSymbolSpace> spaceStack
		) {
			// lh: make this a bit cleverer later to handle variadics.
			if ( variant.SignatureLength != arguments.Count ) {
				return false;
			}

			if ( variant.Signature
				.Zip(
					arguments,
					(type, token) => typeCheck( type, token, spaceStack )
				)
				// if the zip of our two collections is less than the count
				// we started with, at least one given token did not match
				// the sig, so we remove that variant from the potential
				// ones.
				.Where( p => p )
				.Count() != arguments.Count
			) {
				return false;
			}

			return true;
		}

		// given a type from our sig, and the token supplied as a potential
		// argument, see if they match
		bool typeCheck(
			//mysTypes type,
			Type type,
			mysToken token,
			Stack<mysSymbolSpace> spaceStack
		) {
			// we might need to do weird shit with symbols in here?
			// like, you should only really be able to send a quoted symbol
			// in (mainly for ease of reading the code, easy to see reasoning
			// etc., less likely for bugs to occur because of an accidental sig
			// match).

			bool plainAssignable = mysToken.AssignableFrom(
				type,
				token.Type
			);

			bool complexAssignable = false;

			if ( token.Type == typeof(mysSymbol) && !token.Quoted ) {
				mysSymbol s = (token.InternalValue as mysSymbol);
				Type t = s.DeepType( spaceStack );

				if ( t != null ) {
					complexAssignable = mysToken.AssignableFrom( type, t );
				}
			}

			return plainAssignable || complexAssignable;
		}
	}

	public class clrFunctionGroup
	{
		public string GroupName;

		public clrFunctionGroup( string name )
		{
			GroupName = name;
		}

		public static clrFunction Judge(
			List<MethodInfo> Variants,
			List<mysToken> arguments,
			Stack<mysSymbolSpace> spaceStack
		) {
			List<MethodInfo> variants = new List<MethodInfo>( Variants );

			variants.RemoveAll(
				v => !judgeVariant( v.GetParameters(), arguments, spaceStack )
			);

			if ( variants.Count == 0 ) {
				return null;
			} else if ( variants.Count != 1 ) {
				throw new SignatureAmbiguityException();
			}

			return new clrFunction( variants[ 0 ] );
		}

		public static ConstructorInfo Judge(
			List<ConstructorInfo> Variants,
			List<mysToken> arguments,
			Stack<mysSymbolSpace> spaceStack
		) {
			List<ConstructorInfo> variants =
				new List<ConstructorInfo>( Variants );

			variants.RemoveAll(
				v => !judgeVariant( v.GetParameters(), arguments, spaceStack )
			);

			if ( variants.Count == 0 ) {
				return null;
			} else if ( variants.Count != 1 ) {
				throw new SignatureAmbiguityException();
			}

			return variants[ 0 ];
		}

		static bool judgeVariant(
			ParameterInfo[] parameters,
			List<mysToken> arguments,
			Stack<mysSymbolSpace> spaceStack
		) {
			int signatureLength = parameters.Length;
			// lh: make this a bit cleverer later to handle variadics.
			if ( signatureLength != arguments.Count ) {
				return false;
			}

			// issues with symbol?
			for ( int i = 0; i < signatureLength; i++ ) {
				ParameterInfo pi = parameters[ i ];

				Type t;

				mysSymbol symbol = arguments[ i ].InternalValue as mysSymbol;

				if ( symbol != null ) {
					t = symbol.Value( spaceStack ).InternalValue.GetType();
				} else {
					t = arguments[ i ].InternalValue.GetType();
				}

				if ( t != pi.ParameterType ) {
					return false;
				}
			}

			return true;
		}
	}
}
