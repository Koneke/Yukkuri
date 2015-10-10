using System.Linq;
using System.Collections.Generic;

namespace mysharp.Parsing
{
	class StringParser
	{
		public const string StringPlaceholderToken = "STR_LEX";

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
							StringPlaceholderToken
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
}
