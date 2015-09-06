﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace mysharp.Builtins
{
	// might want to move this stuff to its own project?
	// and ref to that?
	// makes sense to keep the sort of "standard library" separate.

	public static class Addition {
		static mysFunctionGroup functionGroup;

		static void setupIntIntVariant() {
			mysBuiltin variant = new mysBuiltin();

			variant.ReturnType = mysTypes.Integral;

			variant.Signature.Add( mysTypes.Integral );
			variant.Signature.Add( mysTypes.Integral );

			variant.Function = (args, state, sss) =>
				new mysIntegral(
					(args[ 0 ] as mysIntegral).Value +
					(args[ 1 ] as mysIntegral).Value
				);

			functionGroup.Variants.Add( variant );
		}

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			setupIntIntVariant();

			mysBuiltin.DefineInGlobal( "+", functionGroup, global );
		}
	}

	public static class Subtraction {
		static mysFunctionGroup functionGroup;

		static void setupIntIntVariant() {
			mysBuiltin variant = new mysBuiltin();

			variant.ReturnType = mysTypes.Integral;

			variant.Signature.Add( mysTypes.Integral );
			variant.Signature.Add( mysTypes.Integral );

			variant.Function = (args, state, sss) =>
				new mysIntegral(
					(args[ 0 ] as mysIntegral).Value -
					(args[ 1 ] as mysIntegral).Value
				);

			functionGroup.Variants.Add( variant );
		}

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			setupIntIntVariant();

			mysBuiltin.DefineInGlobal( "-", functionGroup, global );
		}
	}

	public static class Multiplication {
		static mysFunctionGroup functionGroup;

		static void setupIntIntVariant() {
			mysBuiltin variant = new mysBuiltin();

			variant.ReturnType = mysTypes.Integral;

			variant.Signature.Add( mysTypes.Integral );
			variant.Signature.Add( mysTypes.Integral );

			variant.Function = (args, state, sss) =>
				new mysIntegral(
					(args[ 0 ] as mysIntegral).Value *
					(args[ 1 ] as mysIntegral).Value
				);

			functionGroup.Variants.Add( variant );
		}

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			setupIntIntVariant();

			mysBuiltin.DefineInGlobal( "*", functionGroup, global );
		}
	}

	public static class Division {
		static mysFunctionGroup functionGroup;

		static void setupIntIntVariant() {
			mysBuiltin variant = new mysBuiltin();

			variant.ReturnType = mysTypes.Integral;

			variant.Signature.Add( mysTypes.Integral );
			variant.Signature.Add( mysTypes.Integral );

			variant.Function = (args, state, sss) =>
				new mysIntegral(
					(args[ 0 ] as mysIntegral).Value /
					(args[ 1 ] as mysIntegral).Value
				);

			functionGroup.Variants.Add( variant );
		}

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			setupIntIntVariant();

			mysBuiltin.DefineInGlobal( "/", functionGroup, global );
		}
	}

	public static class Assign {
		static void defineFunction(
			mysSymbol symbol,
			mysFunction f,
			mysSymbolSpace ss
		) {
			mysFunctionGroup fg = null;

			// if symbol defined and of wrong type, undef it
			if (
				ss.Defined( symbol ) &&
				ss.GetValue( symbol ).Type != mysTypes.FunctionGroup
			) {
				// we could just overwrite it with define,
				// but I'd rather be entirely sure that we delete
				// the old value beforehand.
				ss.Undefine( symbol );
			}

			// if we're defined at this point, we know it's a function group
			if  ( ss.Defined( symbol ) ) {
				fg = ss.GetValue( symbol ) as mysFunctionGroup;
			} else {
				// create 
				fg = new mysFunctionGroup();
				fg.Type = mysTypes.FunctionGroup;

				ss.Define( symbol, fg );
				symbol.Type = mysTypes.FunctionGroup;
			}

			mysFunction collision = fg.Variants.FirstOrDefault(
				v => v.Signature
					.Zip( f.Signature, (a, b) => a == b )
					.Count() == v.SignatureLength
			);

			if ( collision != null ) {
				// overwrite a conflicting sig! should probably
				// notify the user about this when it happens.
				fg.Variants.Remove( collision );
			}

			fg.Variants.Add( f );
		}

		public static mysToken Evaluate(
			mysSymbol symbol,
			mysToken value,
			Stack<mysSymbolSpace> spaceStack
		) {
			// NOTICE THIS
			// since each function has it's own internal space
			// before grabbing our reference to the space in which
			// we want to define our symbol, we need to pop the
			// internal off, or we're going to be defining the symbol
			// in our internal space, i.e. it will scope out as soon as
			// we're done. So we pop the internal off, grab our reference
			// to the space outside of that, then push the internal back on.
			mysSymbolSpace top = spaceStack.Pop();
			mysSymbolSpace ss = spaceStack.Peek();

			switch ( value.Type ) {
				case mysTypes.Function:
					defineFunction(
						symbol,
						value as mysFunction,
						spaceStack.Peek()
					);
					break;

				default:
					mysSymbolSpace space = symbol.DefinedIn( spaceStack );
					if ( space != null ) {
						space.Define( symbol, value );
					} else {
						//spaceStack.Peek().Define( symbol, value );
						ss.Define( symbol, value );
					}

					//spaceStack.Peek().Define( symbol, value );
					break;
			}

			spaceStack.Push( top );
			return null;
			//return value;
		}

		public static void Setup( mysSymbolSpace global )
		{
			mysFunctionGroup assign = new mysFunctionGroup();
			mysBuiltin assignVariant = new mysBuiltin();

			assignVariant = new mysBuiltin();
			assignVariant.Signature.Add( mysTypes.Symbol );
			assignVariant.Signature.Add( mysTypes.ANY );

			assignVariant.Function = (args, state, sss) => {
				mysSymbol assignsymbol = args[ 0 ] as mysSymbol;
				mysToken value = args[ 1 ];

				return Evaluate( assignsymbol, value, sss );
			};

			assign.Variants.Add( assignVariant );
			
			mysBuiltin.DefineInGlobal( "def", assign, global );
		}
	}

	public static class Lambda  {
		static void Argumentcheck(
			mysList sig,
			mysList body
		) {
			if ( sig.InternalValues.Count %2 != 0 ) {
				throw new ArgumentException();
			}

			for ( int i = 0; i < sig.InternalValues.Count; i++ ) {
				if ( sig.InternalValues[ i ].Type !=
					( i % 2 == 0
						? mysTypes.Symbol
						: mysTypes.mysType )
				) {
					throw new ArgumentException();
				}
			}
		}

		public static mysToken Evaluate(
			mysList sig,
			mysList body,
			Stack<mysSymbolSpace> sss
		) {
			mysSymbolSpace ss = sss.Peek();

			Argumentcheck( sig, body );

			// define function variant
			mysFunction f = new mysFunction();

			// these two should probably be joined at some point
			for ( int i = 0; i < sig.InternalValues.Count; i++ ) {
				if ( sig.InternalValues[ i ].Type == mysTypes.Symbol ) {
					f.Symbols.Add(
						sig.InternalValues[ i ] as mysSymbol
					);
				} else {
					f.Signature.Add(
						( sig.InternalValues[ i ] as mysTypeToken )
							.Value
					);
				}
			}

			f.Function = body;
			// end define function variant

			return f;
		}

		public static void Setup( mysSymbolSpace global )
		{
			mysFunctionGroup lambda = new mysFunctionGroup();
			mysBuiltin lambdaVariant = new mysBuiltin();

			lambdaVariant = new mysBuiltin();
			lambdaVariant.Signature.Add( mysTypes.List );
			lambdaVariant.Signature.Add( mysTypes.List );

			lambdaVariant.Function = (args, state, sss) => {
				mysSymbolSpace ss = sss.Peek();

				mysList sig = args[ 0 ] as mysList;
				mysList body = args[ 1 ] as mysList;

				return Evaluate( sig, body, sss );
			};

			lambda.Variants.Add( lambdaVariant );

			mysBuiltin.DefineInGlobal( "=>", lambda, global );
		}
	}

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

	public static class NewClrObject
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( mysTypes.String );

			f.Function = (args, state, sss) => {
				mysString type = args[ 0 ] as mysString;

				foreach( Assembly a in state.exposedAssemblies ) {
					if ( a.GetExportedTypes()
						.Any( t => t.FullName == type.Value )
					) {
						return new clrObject(
							Activator.CreateInstance(
								a.GetType( type.Value )
							)
						);
					}
				}

				throw new Exception( "Type not imported." );
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "#new", functionGroup, global );
		}
	}
}

namespace mysharp.Builtins.Flow
{
	public static class If 
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( mysTypes.Boolean );
			f.Signature.Add( mysTypes.List );
			f.Signature.Add( mysTypes.List );

			f.Function = (args, state, sss) => {
				mysBoolean condition = args[ 0 ] as mysBoolean;
				mysList positive = args[ 1 ] as mysList;
				mysList negative = args[ 2 ] as mysList;

				EvaluationMachine em;
				if ( condition.Value ) {
					em = new EvaluationMachine(
						positive.InternalValues,
						state,
						sss
					);
				} else {
					em = new EvaluationMachine(
						negative.InternalValues,
						state,
						sss
					);
				}

				List<mysToken> result = em.Evaluate();

				if ( result.Count == 1 ) {
					return result.Car();
				} else {
					return new mysList( result );
				}
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "if", functionGroup, global );
		}
	}

	public static class When
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( mysTypes.Boolean );
			f.Signature.Add( mysTypes.List );

			f.Function = (args, state, sss) => {
				mysBoolean condition = args[ 0 ] as mysBoolean;
				mysList positive = args[ 1 ] as mysList;

				EvaluationMachine em;
				if ( condition.Value ) {
					em = new EvaluationMachine(
						positive.InternalValues,
						state,
						sss
					);
				} else {
					return null;
				}

				List<mysToken> result = em.Evaluate();

				if ( result.Count == 1 ) {
					return result.Car();
				} else {
					return new mysList( result );
				}
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "when", functionGroup, global );
		}
	}
}

namespace mysharp.Builtins.Comparison
{
	public static class Equals
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( mysTypes.ANY );
			f.Signature.Add( mysTypes.ANY );

			f.Function = (args, state, sss) => {
				return new mysBoolean(
					args[ 0 ].InternalValue.Equals( args[ 1 ].InternalValue )
				);
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "=", functionGroup, global );
		}
	}

	public static class GreaterThan
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( mysTypes.NUMBER );
			f.Signature.Add( mysTypes.NUMBER );

			f.Function = (args, state, sss) => {
				Func<mysToken, mysFloating> toNumber = o =>
					o.Type == mysTypes.Integral
					? ( o as mysIntegral ).Promote()
					: o as mysFloating
				;

				mysFloating first = toNumber( args[ 0 ] );
				mysFloating second = toNumber( args[ 1 ] );

				return new mysBoolean(
					first.Value > second.Value
				);
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( ">", functionGroup, global );
		}
	}
}

namespace mysharp.Builtins.Looping
{
	public static class While
	{
		static mysFunctionGroup functionGroup;

		public static void Setup( mysSymbolSpace global ) {
			functionGroup = new mysFunctionGroup();

			mysBuiltin f = new mysBuiltin();

			f.Signature.Add( mysTypes.List );
			f.Signature.Add( mysTypes.List );

			f.Function = (args, state, sss) => {
				mysList conditional = args[ 0 ] as mysList;

				mysToken finalReturn = null;

				while ( true ) {
					// might want to move this outside somehow, and/or make
					// em non-destructive, so we can call evaluate several times
					// without making a new em all the time.
					// not really expensive to make new ones though I guess, but
					// it does look a bit clunky.
					EvaluationMachine em = new EvaluationMachine(
						conditional.InternalValues,
						state,
						sss
					);

					mysBoolean condition = em.Evaluate().Car() as mysBoolean;

					if ( condition == null ) {
						throw new ArgumentException();
					}

					if ( !condition.Value ) {
						break;
					}

					em = new EvaluationMachine(
						( args[ 1 ] as mysList ).InternalValues,
						state,
						sss
					);

					finalReturn = em.Evaluate().Car();
				}

				return finalReturn;
			};

			functionGroup.Variants.Add( f );

			mysBuiltin.DefineInGlobal( "while", functionGroup, global );
		}
	}
}

namespace mysharp
{
	public static class mysBuiltins {
		public static void Setup(
			mysSymbolSpace global
		) {
			Builtins.Addition.Setup( global );
			Builtins.Subtraction.Setup( global );
			Builtins.Multiplication.Setup( global );
			Builtins.Division.Setup( global );

			Builtins.Lambda.Setup( global );
			Builtins.Assign.Setup( global );

			Builtins.Car.Setup( global );
			Builtins.Cdr.Setup( global );

			Builtins.NewClrObject.Setup( global );

			Builtins.Flow.If.Setup( global );
			Builtins.Flow.When.Setup( global );

			Builtins.Comparison.Equals.Setup( global );
			Builtins.Comparison.GreaterThan.Setup( global );

			Builtins.Looping.While.Setup( global );
		}
	}
}
