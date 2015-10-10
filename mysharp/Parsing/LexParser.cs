using System;
using System.Linq;
using System.Collections.Generic;

namespace mysharp.Parsing
{
	class LexParser
	{
		static Dictionary<string, Type> typeNameDictionary =
			new Dictionary<string, Type>() {
				{ "int", typeof(int) },
				{ "float", typeof(float) },
				{ "bool", typeof(bool) },

				{ "fn", typeof(mysFunction) },
				{ "fng", typeof(mysFunctionGroup) },

				{ "list", typeof(List<mysToken>) },
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

		static int getReplaceTokenIndex( string lex ) {
			if (
				lex.First() != '{' ||
				lex.Last() != '}'
			) {
				return -1;
			}

			string core = lex.Substring( 1, lex.Count() - 2 );

			if ( !core.All( c => char.IsDigit( c ) ) ) {
				return -1;
			}

			return int.Parse( core );
		}


		// parses SIMPLE VALUES, NOT LISTS
		public static mysToken ParseLex(
			mysState state,
			string lex,
			object[] replaces
		) {
			mysToken token = null;

			// if a lex like {0} is passed in, we replace it with
			// a token with the value of the params object at the
			// index given in the lex.
			int replaceToken = getReplaceTokenIndex( lex );
			if ( replaceToken != -1 ) {
				return new mysToken( replaces[ replaceToken ] );
			}

			if ( lex[ 0 ] == ':' ) {
				string type = lex.Substring( 1, lex.Length - 1);
				token = new mysToken(
					typeNameDictionary[ type ]
				);

			} else if ( IsInteger( lex ) ) {
				token = new mysToken( int.Parse( lex ) );

			} else if ( IsFloating( lex ) ) {
				lex = lex.Replace( '.', ',' );
				token = new mysToken( float.Parse( lex ) );

			} else if ( IsValidAccessor( lex ) ) {
				string name = string.Concat( lex.Cdr() );
				token = new mysToken( new clrFunctionGroup( name ) );

			} else if ( IsValidTypeName( lex ) ) {
				string name = string.Concat( lex.Cdr() );

				token = new mysToken(
					Builtins.Clr.ClrTools.GetType(
						state,
						name
					)
				);

			} else {
				if ( IsValidIdentifier( lex ) ) {
					// special case
					if ( lex == "new" ) {
						token = new mysToken(
							new clrFunctionGroup( lex )
						);
					} else {
						token = new mysToken(
							new mysSymbol( lex )
						);
					}
				}
				else {
					throw new FormatException();
				}
			}

			return token;
		}
	}
}
