using System;
using System.Collections.Generic;
using System.Linq;

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

		public static void ForEach<T>(this IEnumerable<T> sequence, Action<T,int> action)
		{
			if(sequence == null) return;
			var i = 0;
			foreach(var item in sequence)
			{
				action(item,i);
				i++;
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

		public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
		{
			return collection == null || collection.Count == 0;
		}

		public static IEnumerable<T> ExceptThoseInSet<T,T2>(this IEnumerable<T> sequence, ISet<T2> exceptThese) where T:T2
		{
			return sequence.Where(i => !exceptThese.Contains(i));
		}
	}
}