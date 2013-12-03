using System.Collections;
using System.Collections.Generic;

namespace Aktris.Internals
{
	/// <summary>A marker interface for enumerations that are immutable.</summary>
	public interface IImmutableEnumerable<out T> :IEnumerable<T>
	{
		//Intentionally left blank 
	}

	public static class IImmutableEnumerable
	{
		public static IImmutableEnumerable<T> Empty<T>()
		{
			return EmptyEnumerable<T>.Instance;
		}

		private class EmptyEnumerable<T> : IImmutableEnumerable<T>
		{
			public static readonly EmptyEnumerable<T> Instance = new EmptyEnumerable<T>();

			private EmptyEnumerable(){}

			public IEnumerator<T> GetEnumerator()
			{
				yield break;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}
}