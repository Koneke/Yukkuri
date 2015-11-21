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

		static bool IsValidTypeName( string lex ) {
			if ( lex.Length < 2 ) {
				return false;
			}

			if ( lex[ 0 ] != '#' ) {
				return false;
			}

			string name = string.Concat( lex.Cdr() );

			// TODO: make actually comply with .NET names?
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

		//======================================

		static mysToken readReplaceToken( string lex ) {
			// if a lex like {0} is passed in, we replace it with
			// a token with the value of the params object at the
			// index given in the lex.
			int replaceToken = getReplaceTokenIndex( lex );

			if ( replaceToken == -1 ) {
				return null;
			}

			return new mysToken( replaces[ replaceToken ] );
		}

		static mysToken readType( string lex ) {
			if ( lex[ 0 ] != ':' ) {
				return null;
			}
			string type = lex.Substring( 1, lex.Length - 1 );

			if ( !typeNameDictionary.ContainsKey( type ) ) {
				return null;
			}

			return new mysToken( typeNameDictionary[ type ] );
		}

		static mysToken readInteger( string lex ) {
			//long result;
			int result;
			//return long.TryParse( lex, out result )
			return int.TryParse( lex, out result )
				? new mysToken( result )
				: null
			;
		}

		static mysToken readFloating( string lex ) {
			double result;

			// can probably be done culturesensitive and nice?
			return double.TryParse( lex.Replace( ".", "," ), out result )
				? new mysToken( result )
				: null
			;
		}

		// only fg atm
		static mysToken readClrAccessor( string lex ) {
			if ( lex.Length < 2 ) {
				return null;
			}

			if ( lex[ 0 ] != '.' ) {
				return null;
			}

			string name = string.Concat( lex.Cdr() );

			if ( !IsValidIdentifier( name ) ) {
				return null;
			}

			return new mysToken( new clrFunctionGroup( name ) );
		}

		static mysToken readClrType( string lex ) {
			if ( !IsValidTypeName( lex ) ) {
				return null;
			}

			return new mysToken(
				Builtins.Clr.ClrTools.GetType(
					state,
					string.Concat( lex.Cdr() )
				)
			);
		}

		//======================================

		static mysState state;
		static object[] replaces;

		static List<Func<string, mysToken>> actions =
			new List<Func<string, mysToken>>()
		{
			readReplaceToken,
			readType,
			readInteger,
			readFloating,
			readClrAccessor,
			readClrType
		};

		// parses SIMPLE VALUES, NOT LISTS
		public static mysToken ParseLex(
			mysState state,
			string lex,
			object[] replaces
		) {
			LexParser.state = state;
			LexParser.replaces = replaces;

			foreach( Func<string, mysToken> transform in actions ) {
				mysToken result = transform( lex );

				if ( result != null ) {
					return result;
				}
			}

			if ( IsValidIdentifier( lex ) ) {
				// special case
				if ( lex == "new" ) {
					return new mysToken(
						new clrFunctionGroup( lex )
					);
				} else {
					return new mysToken(
						new mysSymbol( lex )
					);
				}
			} else {
				throw new FormatException();
			}
		}
	}
}
