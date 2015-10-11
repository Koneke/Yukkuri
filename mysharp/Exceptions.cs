using System;

namespace mysharp
{
	[Serializable]
	class NoSuchSignatureException : Exception
	{
		public NoSuchSignatureException( string message )
			: base( message )
		{
		}
	}

	[Serializable]
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
