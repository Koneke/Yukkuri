using System;
using System.Collections.Generic;
using System.Linq;

namespace mysharp
{
	public class mysParser
	{
		// parses SIMPLE VALUES, NOT LISTS
		public mysToken ParseLex( string s ) {
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

		// todo: given two lists, should return two separate lists
		//       (change return type to List<mysList>), instead of one list of
		//       both given lists. This so we can make sure to parse things the
		//       right order when we give it a toplevel expression.
		// todo: actually no, or well yes, but that still wont cut it.
		//       we really need to make evaluate bunch things before evaluating,
		//       i.e. first decide, using right-to-left, what args are to which
		//       functions, then evaluate things, from left-to-right.
		//       should solve the top-level issue too.
		// jesus fuck why does this function still look like this.
		public mysList Parse( string expression ) {
			List<string> split = expression
				.Replace( "(", " ( " )
				.Replace( ")", " ) " )
				.Replace( "'", " ' " )
				.Split(' ')
				.Where( sub => sub != " " && sub != "" )
				.ToList()
			;

			List<mysToken> tokens = new List<mysToken>();

			bool quote = false;

			for (
				int startToken = 0;
				startToken < split.Count;
				startToken++
			) {
				if ( split[ startToken ] == "(" ) {
					int depth = 0;

					for (
						int endToken = startToken + 1;
						endToken < split.Count;
						endToken++
					) {
						if ( split[ endToken ] == "(" ) {
							depth++;
						} else if ( split[ endToken ] == ")" ) {
							depth--;
							if ( depth == -1 ) {
								int count = endToken - startToken + 1;

								string body = 
									string.Join(
										" ",
										split
											.Skip( startToken + 1 )
											.Take( count - 2 )
									);

								tokens.Add(
									Parse( body )
										.Quote( quote )
								);

								quote = false;

								split.RemoveRange(
									startToken,
									endToken - startToken + 1
								);
								startToken--;
								break;
							}
						}
					}
				} else if ( split[ startToken ] == "'" ) {
					quote = true;

					split.RemoveAt( startToken );
					startToken--;
				} else {
					// simple value
					tokens.Add(
						ParseLex( split[ startToken ] )
							.Quote( quote )
					);

					quote = false;

					split.RemoveAt( startToken );
					startToken--;
					var a = 0; // just for breaking
				}
			}

			return new mysList( tokens );
		}
	}

}
