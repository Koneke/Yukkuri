using System;

namespace mysharp
{
	class NoSuchSignatureException : Exception
	{
		public NoSuchSignatureException( string message )
			: base( message )
		{
		}
	}

	class SignatureAmbiguityException : Exception {
		public SignatureAmbiguityException()
		{
		}

		public SignatureAmbiguityException( string message )
			: base( message )
		{
		}
	}

}
