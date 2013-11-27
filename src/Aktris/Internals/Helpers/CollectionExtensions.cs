using System;
using System.Collections.Generic;

namespace Aktris.Internals.Helpers
{
	public static class CollectionExtensions
	{
		public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
		{
			if(sequence == null) return;
			foreach(var item in sequence)
			{
				action(item);
			}
		}

		public static void ForEach<T, TArg>(this IEnumerable<T> sequence, TArg arg, Action<TArg, T> action)
		{
			if(sequence == null) return;
			foreach(var item in sequence)
			{
				action(arg, item);
			}
		}

	}
}