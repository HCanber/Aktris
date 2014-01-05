using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Aktris.Internals
{
	/// <summary>A marker interface for enumerations that are immutable.</summary>
	public interface IImmutableEnumerable<out T> :IEnumerable<T>
	{
		int Count { get; }
	}

	public static class IImmutableEnumerable
	{
		public static IImmutableEnumerable<T> Empty<T>()
		{
			return EmptyEnumerable<T>.Instance;
		}

		public static IImmutableEnumerable<T> ToImmutableEnumerable<T>(this IEnumerable<T> sequence)
		{
			return new ImmutableEnumerable<T>(sequence);
		}

		private class EmptyEnumerable<T> : IImmutableEnumerable<T>
		{
			public static readonly EmptyEnumerable<T> Instance = new EmptyEnumerable<T>();

			private EmptyEnumerable(){}

			public int Count { get { return 0; } }

			public IEnumerator<T> GetEnumerator()
			{
				yield break;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		private class ImmutableEnumerable<T> : IImmutableEnumerable<T>
		{
			private List<T> _source;

			public ImmutableEnumerable(IEnumerable<T> source)
			{
				_source = source.ToList();
			}

			public int Count { get { return _source.Count; }}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public IEnumerator<T> GetEnumerator()
			{
				return _source.GetEnumerator();
			}
		}
	}
}