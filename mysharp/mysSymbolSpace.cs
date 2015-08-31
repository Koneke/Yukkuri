using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mysharp
{
	public class mysSymbolSpace
	{
		public static mysToken GetAndEvaluateSymbol( 
			string symbolString,
			Stack<mysSymbolSpace> spaceStack
		) {
			return EvaluateSymbol(
				GetSymbol( symbolString, spaceStack ),
				spaceStack
			);
		}

		public static mysToken EvaluateSymbol(
			mysSymbol symbol,
			Stack<mysSymbolSpace> spaceStack
		) {
			Stack<mysSymbolSpace> evaluationStack = spaceStack.Clone();

			while ( evaluationStack.Count > 0 ) {
				mysSymbolSpace space = evaluationStack.Pop();
				if ( space.Defined( symbol ) ) {
					return space.GetValue( symbol );
				}
			}

			throw new ArgumentException( "Symbol isn't defined." );
		}

		public static mysSymbol GetSymbol(
			string symbolString,
			Stack<mysSymbolSpace> spaceStack
		) {
			Stack<mysSymbolSpace> evaluationStack = spaceStack.Clone();

			while ( evaluationStack.Count > 0 ) {
				mysSymbolSpace space = evaluationStack.Pop();

				mysSymbol symbol = space.Values.Keys
					.FirstOrDefault( s => s.ToString() == symbolString );

				if ( symbol != null ) {
					return symbol;
				}
			}

			return null;
		}

		private Dictionary<string, mysSymbol> symbols =
			new Dictionary<string, mysSymbol>();

		public mysSymbol Create( string symbolString ) {
			if ( symbols.ContainsKey( symbolString ) ) {
				throw new ArgumentException("Symbol already exists.");
			}

			mysSymbol newSymbol = new mysSymbol( symbolString );
			symbols.Add( symbolString, newSymbol );

			return newSymbol;
		}

		public void Define( mysSymbol symbol, mysToken value ) {
			Values.Add( symbol, value );
		}

		public void Undefine( mysSymbol symbol ) {
			Values.Remove( symbol );
		}

		public bool Defined( mysSymbol symbol ) {
			return Values.ContainsKey( symbol );
		}

		public mysToken GetValue( mysSymbol symbol ) {
			return Values[ symbol ];
		}

		Dictionary<mysSymbol, mysToken> Values;

		public mysSymbolSpace() {
			Values = new Dictionary<mysSymbol, mysToken>();
		}
	}
}
