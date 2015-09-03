using System;
using System.Collections.Generic;
using System.Linq;

namespace mysharp
{
	public class mysParser
	{
		static Dictionary<string, mysTypes> typeNameDictionary =
			new Dictionary<string, mysTypes>() {
				{ "int",  mysTypes.Integral },
				{ "float",  mysTypes.Floating },

				{ "fn",  mysTypes.Function },
				{ "fng",  mysTypes.FunctionGroup },

				{ "list",  mysTypes.List },

				{ "sym",  mysTypes.Symbol },

				{ "type",  mysTypes.mysType },
				{ "any",  mysTypes.ANY },
			};

		static string GetSign( string lex ) {
			string sign = null;

			if ( lex[ 0 ] == '-' || lex[ 0 ] == '+' ) {
				sign = "" + lex[ 0 ];
			}

			return sign;
		}

		static bool IsInteger( string lex ) {
			string sign = GetSign( lex );

			return sign == null
				? lex.All( char.IsDigit )
				: ( lex.Count() > 1 && lex.Skip( 1 ).All( char.IsDigit ) )
			;
		}

		static bool IsFloating( string lex ) {
			string sign = GetSign( lex );

			if ( lex.Count( c => c == '.' || c == ',' ) != 1 ) {
				return false;
			}

			lex = lex
				.Replace( ".", "" )
				.Replace( ",", "" )
			;

			if ( sign == null ) {
				return lex.All( char.IsDigit );
			} else {
				// less than three chars when sign is null means either
				// lacking digit, or ./,
				if ( lex.Count() < 3 ) {
					return false;
				}

				lex = lex.Cdr().StringJoin();

				return lex.All( char.IsDigit );
			}
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
