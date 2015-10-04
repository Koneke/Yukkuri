using System;
using System.Linq;
using System.Collections.Generic;

namespace mysharp
{
	public class mysParser
	{
		static Dictionary<string, Type> typeNameDictionary =
			new Dictionary<string, Type>() {
				{ "int", typeof(int) },
				{ "float", typeof(float) },
				{ "bool", typeof(bool) },

				{ "fn", typeof(mysFunction) },
				{ "fng", typeof(mysFunctionGroup) },

				{ "list", typeof(mysList) },
				{ "str", typeof(string) },

				{ "sym", typeof(mysSymbol) },

				{ "type", typeof(Type) },
				{ "any", typeof(ANY) },
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

		static bool IsValidIdentifier( string lex ) {
			List<char> allowed = new List<char>();
			allowed.AddRange(
				"abcdefghijklmnopqrstuvwxyz".ToCharArray()
			);
			allowed.AddRange(
				"ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()
			);
			allowed.AddRange(
				"+-*/><=?!-_#.".ToCharArray()
			);

			return lex.All( c => allowed.Contains( c ) );
		}

		static bool IsValidAccessor( string lex ) {
			if ( lex.Length < 2 ) {
				return false;
			}

			if ( lex[ 0 ] != '.' ) {
				return false;
			}

			string name = string.Concat( lex.Cdr() );

			return IsValidIdentifier( name );
		}

		static bool IsValidTypeName( string lex ) {
			if ( lex.Length < 2 ) {
				return false;
			}

			if ( lex[ 0 ] != '#' ) {
				return false;
			}

			string name = string.Concat( lex.Cdr() );

			// todo!: make actually comply with .NET names
			return IsValidIdentifier( name );
		}

		// parses SIMPLE VALUES, NOT LISTS
		public static mysToken ParseLex( mysState state, string lex ) {
			mysToken token = null;

			if ( lex[ 0 ] == ':' ) {
				string type = lex.Substring( 1, lex.Length - 1);
				token = new mysToken( typeNameDictionary[ type ] );

			} else if ( IsInteger( lex ) ) {
				token = new mysToken( int.Parse( lex ) );

			} else if ( IsFloating( lex ) ) {
				lex = lex.Replace( '.', ',' );
				token = new mysToken( float.Parse( lex ) );

			} else if ( IsValidAccessor( lex ) ) {
				string name = string.Concat( lex.Cdr() );
				token = new clrFunctionGroup( name );

			} else if ( IsValidTypeName( lex ) ) {
				string name = string.Concat( lex.Cdr() );
				token = new clrFunctionGroup( name );

				token = new mysToken(
					Builtins.Clr.ClrTools.GetType(
						state,
						name
					)
				);

			} else {
				if ( IsValidIdentifier( lex ) ) {
					token = new mysSymbol( lex );
				}
				else {
					throw new FormatException();
				}
			}

			return token;
		}

		// made to parse *ONE* statement (i.e., like, one line of REPL)
		// (even if that contains several expressions, like (f 2)(g 3))
		// might also sort of make this less nested....
		// this is a nested class, with another nested class inside..
		class ParseMachine
		{
			// passed in so we have access to exposed assemblies
			mysState state;

			public List<mysToken> Tokens;

			List<string> expression;
			int current;
			bool quote;

			Queue<string> stringQueue;

			public ParseMachine(
				mysState state,
				string expression,
				Queue<string> inheritedStringQueue = null
			) {
				this.state = state;

				stringQueue = inheritedStringQueue ?? new Queue<string>();

				this.expression =
					prepare( expression )
					.Split(' ')
					.Where( sub => sub != " " && sub != "" )
					.ToList()
				;

				current = 0;
				Tokens = new List<mysToken>();
				quote = false;
			}

			string prepare( string expression ) {
				return
					( new StringParser( expression, stringQueue ) ).Parse()
					.Replace( "\r", " " )
					.Replace( "\n", " " )
					.Replace( "(", " ( " )
					.Replace( ")", " ) " )
					.Replace( "[", " [ " )
					.Replace( "]", " ] " )
					.Replace( "'", " ' " )
				;
			}

			class StringParser
			{
				string expression;
				Queue<string> stringQueue;

				bool inString;
				string currentString; // current string we're building
				string outString;
				int current;

				public StringParser(
					string expression,
					Queue<string> stringQueue
				) {
					this.expression = expression;
					this.stringQueue = stringQueue;
				}

				public string Parse() {
					inString = false;
					currentString = "";
					outString = expression;
					current = 0;

					while ( canStep() ) {
						step();
					}

					return outString;
				}

				bool canStep() {
					return current < expression.Count();
				}

				bool currentIsQuote() {
					if ( expression[ current ] != '"' ) {
						return false;
					}

					if ( current > 0 && expression[ current - 1 ] == '\\' ) {
						return false;
					}

					return true;
				}

				void step() {
					if ( currentIsQuote() ) {
						inString = !inString;

						if ( !inString ) {
							stringQueue.Enqueue(
								currentString.Replace( "\\\"", "\"" )
							);

							outString = outString
								.Replace(
									"\"" + currentString + "\"",
									"STR_LEX"
								);
							currentString = "";
						}
					} else {
						if ( inString ) {
							currentString += expression[ current ];
						}
					}

					current++;
				}
			}

			public bool CanStep() {
				return current < expression.Count;
			}

			void removeCurrent() {
				expression.RemoveAt( current );
				current--;
			}

			void eat( mysToken token ) {
				Tokens.Add( token.Quote( quote ) );

				quote = false;

				removeCurrent();
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
						removeCurrent();
						break;

					case "STR_LEX":
						eat( new mysToken( stringQueue.Dequeue() ) );
						break;

					// simple value
					default:
						eat( ParseLex( state, expression[ current ] ) );
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

				ParseMachine pm = new ParseMachine( state, body, stringQueue );
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

		public List<mysToken> Parse( mysState state, string expression ) {
			ParseMachine pm = new ParseMachine( state, expression );

			while ( pm.CanStep() ) {
				pm.Step();
			}

			return pm.Tokens;
		}
	}
}
