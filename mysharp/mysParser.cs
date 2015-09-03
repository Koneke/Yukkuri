using System;
using System.Collections.Generic;
using System.Linq;

namespace mysharp
{
	public class mysParser
	{
		// parses SIMPLE VALUES, NOT LISTS
		public static mysToken ParseLex( string s ) {
			mysToken token = null;

			if ( s[ 0 ] == ':' ) {
				switch ( s.Substring( 1, s.Length - 1 ) ) {
					case "int":
						token = new mysTypeToken( mysTypes.Integral );
						break;
					default:
						throw new ArgumentException();
				}
			} else if ( s.All( c => c >= '0' && c <= '9' ) ) {
				token = new mysIntegral( long.Parse( s ) );
			} else if ( s.All( c =>
				c == '.' || c == ',' ||
				(c >= '0' && c <= '9' ) )
			) {
				s = s.Replace( '.', ',' );
				token = new mysFloating( double.Parse( s ) );
			} else {
				token = new mysSymbol( s );
			}

			return token;
		}

		class ParseMachine
		{
			public List<mysToken> Tokens;
			List<string> expression;
			int current;
			bool quote;

			public ParseMachine(
				string expression
			) {
				this.expression = expression
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

			int findBuddy( string character ) {
				int depth = 0;

				bool inQuote = false;

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
					if ( !inQuote ) {
						if ( expression[ endToken ] == character ) {
							depth++;
						} else if ( expression[ endToken ] == matching ) {
							depth--;
							if ( depth == -1 ) {
								return endToken;
							}
						}
					} else if ( expression[ endToken ] == "\"" ) {
						if ( expression[ endToken - 1 ] != "\\" ) {
							inQuote = !inQuote;
						}
					}
				}

				throw new FormatException();
			}

			void makeList( int length ) {
				string body = string.Join(
					" ", expression.Between( current + 1, length - 2)
				);

				ParseMachine pm = new ParseMachine( body );
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

			public bool CanStep() {
				return current < expression.Count;
			}

			public void Step() {
				string token = expression[ current ];

				int end, count;

				switch ( token ) {
					case "(":
						end = findBuddy( token );
						count = end - current + 1;

						makeList( count );
						break;

					case "[":
						quote = true;

						end = findBuddy( token );
						count = end - current + 1;

						makeList( count );
						break;

					case "'":
						quote = true;

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
