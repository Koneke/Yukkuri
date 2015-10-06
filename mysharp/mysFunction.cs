using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace mysharp
{
	public class mysFunction// : mysToken
	{
		// lh: A function is in essence just a list that we interpret as a
		//     parseblock when the function is called upon, making sure to
		//     substitute in our passed values.

		public List<Type> Signature;
		public List<mysSymbol> Symbols;

		public int SignatureLength {
			get { return Signature.Count; }
		}

		public override string ToString()
		{
			return string.Format(
				"(fn: sig: [{0}])",
				string.Join(
					", ",
					Signature.Select( s => s.ToString() )
				)
			);
		}

		public mysList Function;

		public mysFunction()
		{
			Signature = new List<Type>();
			Symbols = new List<mysSymbol>();

			// consider making this the "main value" of the token
			Function = new mysList();
			//InternalValue = this;
		}

		public virtual List<mysToken> Call(
			List<mysToken> arguments,
			mysState state,
			Stack<mysSymbolSpace> spaceStack
		) {
			arguments = arguments.Select( t =>
				t.Type == typeof(mysSymbol) && !t.Quoted
				? ( t as mysSymbol ).Value( spaceStack )
				: t
			).ToList();

			// future, cache somehow?
			mysSymbolSpace internalSpace = new mysSymbolSpace();

			Symbols.DoPaired(
				arguments,
				(s, a) => internalSpace.Define( s, a )
			);

			spaceStack.Push( internalSpace );

			EvaluationMachine em = new EvaluationMachine(
				Function.InternalValues,
				state,
				spaceStack
			);
			List<mysToken> result = em.Evaluate();

			spaceStack.Pop();

			return result;
		}
	}

	public class clrFunction : mysToken {
		MethodInfo method;

		public int SignatureLength {
			get {
				return method.GetParameters().Length;
			}
		}

		public clrFunction( MethodInfo mi )
		{
			method = mi;
			InternalValue = this;
		}

		// arg 1 function
		// arg 2 instance (with . operator)
		// arg 3+ arguments
		public List<mysToken> Call(
			mysToken target,
			List<mysToken> arguments,
			mysState state,
			Stack<mysSymbolSpace> spaceStack
		) {
			object targetObject = null;

			if ( target.Type == typeof(object) ) {
				targetObject = target.InternalValue;
			}

			List<object> realArguments = new List<object>();

			foreach ( mysToken t in arguments ) {
				mysToken current = t;

				while ( current.Type == typeof(mysSymbol) ) {
					current = (current as mysSymbol).Value( spaceStack );
				}

				realArguments.Add( current.InternalValue );
			}

			object result = method.Invoke(
				targetObject,
				realArguments.ToArray()
			);

			if ( result == null ) {
				return null;
			}

			return new List<mysToken>() {
				Builtins.Clr.ClrTools.ConvertClrObject( result )
			};
		}
	}
}
