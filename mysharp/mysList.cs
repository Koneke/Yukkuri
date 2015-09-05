﻿using System.Collections.Generic;

namespace mysharp
{
	public class mysList : mysToken
	{
		public List<mysToken> InternalValues;

		public mysList( bool quoted = false )
			: this( new List<mysToken>(), quoted ) {
		}

		public mysList( List<mysToken> list, bool quoted = false )
			: base ( null, mysTypes.List )
		{
			Quoted = quoted;
			InternalValues = new List<mysToken>( list );
		}

		public override string ToString()
		{
			return string.Format(
				"({1}list: {0})",
				string.Join(", ", InternalValues),
				Quoted ? "q " : ""
			);
		}
	}
}
