using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace mysharp
{
	public static class Extensions
	{
		public static void DoPaired<T, U>(
			this IEnumerable<T> t,
			IEnumerable<U> other,
			Action<T, U> action
		) {
			for ( int i = 0; i < Math.Min( t.Count(), other.Count() ); i++ ) {
				action( t.ElementAt( i ), other.ElementAt( i ) );
			}
		}

		public static Stack<T> Clone<T>(this Stack<T> stack) {
			Contract.Requires( stack != null );
			return new Stack<T>( new Stack<T>( stack ) );
		}

		public static IEnumerable<T> Between<T>(
			this IEnumerable<T> enumerable,
			int first,
			int count
		) {
			return enumerable
				.Skip( first )
				.Take( count );
		}

		public static T Car<T>(
			this IEnumerable<T> enumerable
		) {
			return enumerable.FirstOrDefault();
		}

		public static IEnumerable<T> Cdr<T>(
			this IEnumerable<T> enumerable
		) {
			return enumerable.Skip( 1 );
		}

		public static string StringJoin<T>(
			this IEnumerable<T> enumerable,
			string separator = ""
		) {
			return string.Join( separator, enumerable );
		}
	}

}
