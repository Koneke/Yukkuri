using System;
using System.Collections.Generic;
using System.Linq;

namespace mysharp
{
	public class mysParser
	{
		static Dictionary<string, mysTypes> typeNameDictionary =
			new Dictionary<string, mysTypes>() {
				{ "int", mysTypes.Integral },
				{ "float", mysTypes.Floating },

				{ "fn", mysTypes.Function },
				{ "fng", mysTypes.FunctionGroup },

				{ "list", mysTypes.List },
				{ "str", mysTypes.String },

				{ "sym", mysTypes.Symbol },

				{ "type", mysTypes.mysType },
				{ "any", mysTypes.ANY },
			};

		static bool IsInteger( string lex ) {
			long result;
			return long.TryParse( lex, out result );
		}

		static bool IsFloating( string lex ) {
			double result;
			// can probably be done culturesensitive and nice?
			return double.TryParse(
				lex.Replace( ".", "," ),
				out result
			);
		}

		// parses SIMPLE VALUES, NOT LISTS
		public static mysToken ParseLex( string lex ) {
			mysToken token = null;

			if ( lex[ 0 ] == ':' ) {
				string type = lex.Substring( 1, lex.Length - 1);
				token = new mysTypeToken( typeNameDictionary[ type ] );

			} else if ( IsInteger( lex ) ) {
				token = new mysIntegral( long.Parse( lex ) );

			} else if ( IsFloating( lex ) ) {
				lex = lex.Replace( '.', ',' );
				token = new mysFloating( double.Parse( lex ) );

			} else {
				token = new mysSymbol( lex );
			}

			return token;
		}

		class ParseMachine
		{
			public List<mysToken> Tokens;
			List<string> expression;
			int current;
			bool quote;

			Queue<string> stringQueue;

			public ParseMachine(
				string expression,
				Queue<string> inheritedStringQueue = null
			) {
				stringQueue = inheritedStringQueue ?? new Queue<string>();

				this.expression =
					parseStrings( expression )
					.Replace( "(", " ( " )
					.Replace( ")", " ) " )
					.Replace( "[", " [ " )
					.Replace( "]", " ] " )
					.Replace( "'", " ' " )
					.Split(' ')
					.Where( sub => sub != " " && sub != "" )
					.ToList()
				;

				current = 0;

				Tokens = new List<mysToken>();

				quote = false;
			}

			string parseStrings( string expression ) {
				bool inString = false;
				string currentString = "";

				string expressionCopy = expression;

				for ( int i = 0; i < expression.Count(); i++ ) {
					if ( expression[ i ] == '"' ) {
						if ( i > 0 && expression[ i - 1 ] == '\\' ) {
							currentString += expression[ i ];
						} else {
							inString = !inString;

							if ( !inString ) {
								stringQueue.Enqueue(
									currentString.Replace( "\\\"", "\"" )
								);

								expressionCopy = expressionCopy
									.Replace(
										"\"" + currentString + "\"",
										"STR_LEX"
									);
								currentString = "";
							}
						}
					} else {
						if ( inString ) {
							currentString += expression[ i ];
						}
					}
				}

				return expressionCopy;
			}

			public bool CanStep() {
				return current < expression.Count;
			}

			public void Step() {
				string token = expression[ current ];

				switch ( token ) {
					case "(":
						makeList();
						break;

					case "[":
						quote = true;
						makeList();
						break;

					case "'":
						quote = true;

						expression.RemoveAt( current );
						current--;
						break;

					case "STR_LEX":
						Tokens.Add(
							new mysString( stringQueue.Dequeue() )
								.Quote( quote )
						);

						quote = false;

						expression.RemoveAt( current );
						current--;
						break;

					default:
						// simple value
						Tokens.Add(
							ParseLex( expression[ current ] )
								.Quote( quote )
						);

						quote = false;

						expression.RemoveAt( current );
						current--;
						break;
				}

				current++;
			}

			int findBuddy( string character ) {
				int depth = 0;

				string matching;
				switch ( character ) {
					case "(": matching = ")"; break;
					case "[": matching = "]"; break;
					default: throw new FormatException();
				}

				for (
					int endToken = current + 1;
					endToken < expression.Count;
					endToken++
				) {
					if ( expression[ endToken ] == character ) {
						depth++;
					} else if ( expression[ endToken ] == matching ) {
						depth--;
						if ( depth == -1 ) {
							return endToken;
						}
					}
				}

				throw new FormatException();
			}

			void makeList() {
				int length = findBuddy( expression[ current ] ) - current + 1;

				string body = string.Join(
					" ", expression.Between( current + 1, length - 2)
				);

				ParseMachine pm = new ParseMachine( body, stringQueue );
				while ( pm.CanStep() ) {
					pm.Step();
				}

				List<mysToken> bodyTokens = pm.Tokens;

				mysList list = new mysList(
					bodyTokens,
					quote
				);
				Tokens.Add( list );

				quote = false;

				expression.RemoveRange(
					current,
					length
				);

				current--;
			}

		}

		public List<mysToken> Parse( string expression ) {
			ParseMachine pm = new ParseMachine( expression );

			while ( pm.CanStep() ) {
				pm.Step();
			}

			return pm.Tokens;
		}
	}
}
