using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace mysharp
{
	public class clrUtilities
	{
		public static clrFunction getClrFunctionVariant(
			Type targetType,
			string name,
			List<mysToken> args,
			Stack<mysSymbolSpace> spaceStack
		) {
			List<MethodInfo> variants = targetType
				.GetMethods()
				.Where( m => m.Name == name )
				.ToList()
			;

			int signatureLength = variants
				.Max( v => v.GetParameters().Length );

			clrFunction f = null;

			List<mysToken> arguments = args;

			for ( int j = signatureLength; j >= 0; j-- ) {
				f = clrFunctionGroup.Judge( variants, arguments, spaceStack );

				if ( f != null ) {
					// escape early if we have a positive match
					break;
				}

				arguments.RemoveAt( arguments.Count - 1 );
			}

			return f;
		}
	}
}
