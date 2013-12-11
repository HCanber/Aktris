using System;
using System.Runtime.CompilerServices;

namespace Aktris.Internals
{
	public static class PatternMatcher
	{
		/// <summary>If the item is of the specified type then the handler is invoked and true is returned. </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Match<T>(object item, Action<T> handler) where T : class
		{
			var m = item as T;
			if(m == null) return false;
			handler(m);
			return true;
		}

		/// <summary>If the item is of any of the specified types then the handler is invoked and true is returned. </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Match<T1, T2>(object item, Action<object> handler) where T1 : class
		{
			if(item is T1 || item is T2)
			{
				handler(item);
				return true;
			}
			return false;
		}

		/// <summary>If the item is of any of the specified types then the handler is invoked and true is returned. </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Match<T1, T2, T3>(object item, Action<object> handler) where T1 : class
		{
			if(item is T1 || item is T2 || item is T3)
			{
				handler(item);
				return true;
			}
			return false;
		}

		/// <summary>If the item is of any of the specified types then the handler is invoked and true is returned. </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Match<T1, T2, T3, T4>(object item, Action<object> handler) where T1 : class
		{
			if(item is T1 || item is T2 || item is T3 || item is T4)
			{
				handler(item);
				return true;
			}
			return false;
		}

		/// <summary>If the item is of any of the specified types then the handler is invoked and true is returned. </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Match<T1, T2, T3, T4, T5>(object item, Action<object> handler) where T1 : class
		{
			if(item is T1 || item is T2 || item is T3 || item is T4 || item is T5)
			{
				handler(item);
				return true;
			}
			return false;
		}


		/// <summary>If the item is of the specified type and the predicate returns true then the handler is invoked and true is returned. </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Match<T>(object item, Predicate<T> predicate, Action<T> handler) where T : class
		{
			var m = item as T;
			if(m == null) return false;
			if(!predicate(m)) return false;
			handler(m);
			return true;
		}

		/// <summary>Calls the handler if the item is null and then returns true. If it is not null false is returned.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool MatchNull(object item, Action handler)
		{
			if(item==null)
			{
				handler();
				return true;
			}
			return false;
		}

		/// <summary>Calls the handler for any object and then returns true.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool MatchAll(object item, Action<object> handler)
		{
			handler(item);
			return true;
		}

		/// <summary> Macthes all messages and returns true. </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SwallowAll(object item)
		{
			return true;
		}
	}

	public class PatternMatcher<TRetVal>
	{
		private PatternMatcher<TRetVal> _instance = new PatternMatcher<TRetVal>();

		/// <summary>If the item is of the specified type then the handler is invoked and true is returned. </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TRetVal Match<T>(object item, Func<T, TRetVal> handler) where T : class
		{
			var m = item as T;
			if(m == null) return default(TRetVal);
			return handler(m);
		}

		/// <summary>If the item is of the specified type and the predicate returns true then the handler is invoked and true is returned. </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TRetVal Match<T>(object item, Predicate<T> predicate, Func<T, TRetVal> handler) where T : class
		{
			var m = item as T;
			if(m == null) return default(TRetVal);
			if(!predicate(m)) return default(TRetVal);
			return handler(m);
		}

		public static TRetVal MatchAll(object item, Func<object, TRetVal> handler)
		{
			return handler(item);
		}
	}
}