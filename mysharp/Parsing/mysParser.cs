using System;
using System.Linq;
using System.Collections.Generic;

namespace mysharp.Parsing
{
	// made to parse *ONE* statement (i.e., like, one line of REPL)
	// (even if that contains several expressions, like (f 2)(g 3))
	class ParseMachine
	{
		public static List<mysToken> Parse(
			mysState state,
			string expression,
			object[] replaces = null
		) {
			ParseMachine pm = new ParseMachine(
				state,
				expression,
				replaces: replaces
			);

			while ( pm.CanStep() ) {
				pm.Step();
			}

			return pm.Tokens;
		}

		// ==================================================

		// passed in so we have access to exposed assemblies
		mysState state;

		public List<mysToken> Tokens;

		List<string> expression;
		int current;
		bool quote;

		Queue<string> stringQueue;

		object[] replaces;

		ParseMachine(
			mysState state,
			string expression,
			Queue<string> inheritedStringQueue = null,
			object[] replaces = null
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

			this.replaces = replaces;
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

				case StringParser.StringPlaceholderToken:
					eat( new mysToken( stringQueue.Dequeue() ) );
					break;

				// simple value
				default:
					eat(
						LexParser.ParseLex(
							state,
							expression[ current ],
							replaces
						)
					);
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

			ParseMachine pm = new ParseMachine(
				state,
				body,
				stringQueue,
				replaces
			);
			while ( pm.CanStep() ) {
				pm.Step();
			}

			List<mysToken> bodyTokens = pm.Tokens;

			Tokens.Add( new mysToken( bodyTokens ).Quote( quote ) );

			quote = false;

			expression.RemoveRange(
				current,
				length
			);

			current--;
		}

	}
}
