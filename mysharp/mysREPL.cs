﻿using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace mysharp
{
	public class mysREPL
	{
		public mysState State;

		mysParser parser;
		bool quit;
		bool strict;
		string accumulatedInput;

		public mysREPL() {
			parser = new mysParser();
			State = new mysState();
		}

		void REPLstart() {
			quit = false;
			strict = false;
			accumulatedInput = "";
		}

		// loop loop, I know...
		public void REPLloop() {
			REPLstart();

			while ( !quit ) {
				// show standard prompt > if we are not currently continuing a
				// multiline command, else show the continuation prompt .
				Console.Write(
					accumulatedInput.Count() == 0
					? " > "
					: " . "
				);

				string input = Console.ReadLine();

				switch ( input ) {
					case "(clear)":
						Console.Clear();
						break;

					case "(nuke!)":
						State = new mysState();

						REPLstart();

						Console.WriteLine(
							">> Cleared execution state, REPL status, "+
							"parser status. All to new. <<\n"
						);
						break;

					case "(quit)":
						quit = true;
						break;

					case "(strict)":
						strict = !strict;
						Console.WriteLine( "Strict is now {0}.\n", strict );
						break;

					default:
						handleInput( input );
						break;
				}
			}
		}

		public List<mysToken> Evaluate( string expression ) {
			try {
				List<mysToken> parsed = parser.Parse( State, expression );
				return State.Evaluate( parsed );

			} catch (Exception e) when ( !strict ) {
				Console.WriteLine( e.Message + "\n" );
				return null;
			}
		}
		
		public void Print( List<mysToken> output ) {
			if ( output != null ) {
				string outputstring = string.Join( ", ", output );

				if ( outputstring != "" ) {
					Console.WriteLine( outputstring );
				}
			}

			Console.WriteLine( "Ok.\n" );
		}

		void handleInput( string input ) {
			if ( input == "" ) {
				return;
			}

			if ( input.Last() == '\\') {
				input = input.Substring( 0, input.Length - 1);
				accumulatedInput += input;
				return;
			} else {
				accumulatedInput += input;
			}

			accumulatedInput = accumulatedInput
				.Replace( "\t", " " )
			;

			List<mysToken> output = Evaluate( accumulatedInput );

			Print( output );

			accumulatedInput = "";
		}

		public void ExposeTo( Assembly a ) {
			State.ExposeTo( a );
		}
	}
}