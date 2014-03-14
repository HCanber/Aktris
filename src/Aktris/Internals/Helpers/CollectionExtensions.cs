using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

		public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action, Action<T,Exception> exceptionHandler)
		{
			if(sequence == null) return;
			foreach(var item in sequence)
			{
				try
				{
					action(item);
				}
				catch(Exception e)
				{
					exceptionHandler(item, e);
				}
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

		public static IReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> sequence)
		{
			if(sequence==null)return EmptyReadonlyCollection<T>.Instance;
			var list = sequence.ToList();
			if(list.Count == 0) return EmptyReadonlyCollection<T>.Instance;
			return new ReadOnlyCollection<T>(list);
		}

		public static bool IsEmpty<T>(this ICollection<T> list)
		{
			return list.Count == 0;
		}

		public static bool IsNotEmpty<T>(this ICollection<T> list)
		{
			return list.Count > 0;
		}
	}
}