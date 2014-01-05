using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Aktris.Internals
{
	public static class InterlockedSpin
	{
		/// <summary>
		/// Atomically updates the int <paramref name="reference"/> by calling <paramref name="updater"/> to get the new value.
		/// Note that <paramref name="updater"/> may be called many times so it should be idempotent.
		/// </summary>
		/// <returns>The updated value.</returns>
		public static int Swap(ref int reference, Func<int, int> updater)
		{
			var spinWait = new SpinWait();
			while(true)
			{
				var current = reference;
				var updated = updater(current);
				if(CompareExchange(ref reference, current, updated)) return updated;
				spinWait.SpinOnce();
			}
		}

		/// <summary>
		/// Atomically updates the object <paramref name="reference"/> by calling <paramref name="updater"/> to get the new value.
		/// Note that <paramref name="updater"/> may be called many times so it should be idempotent.
		/// </summary>
		/// <returns>The updated value.</returns>
		public static T Swap<T>(ref T reference, Func<T, T> updater) where T : class
		{
			var spinWait = new SpinWait();
			while(true)
			{
				var current = reference;
				var updated = updater(current);
				if(CompareExchange(ref reference, current, updated)) return updated;
				spinWait.SpinOnce();
			}
		}

		/// <summary>
		/// Atomically updates the object <paramref name="reference"/> by calling <paramref name="updater"/> to get the new value.
		/// Note that <paramref name="updater"/> may be called many times so it should be idempotent.
		/// </summary>
		/// <returns>The updated value. <paramref name="currentValue"/> is set to the value of <paramref name="reference"/> before it was updated.</returns>
		public static T Swap<T>(ref T reference,out T currentValue, Func<T, T> updater) where T : class
		{
			var spinWait = new SpinWait();
			while(true)
			{
				currentValue = reference;
				var updated = updater(currentValue);
				if(CompareExchange(ref reference, currentValue, updated)) return updated;
				spinWait.SpinOnce();
			}
		}


		/// <summary>
		/// Atomically updates the int <paramref name="reference"/> by calling <paramref name="updateIfTrue"/> to get the new value.
		/// <paramref name="updateIfTrue"/> returns a Tuple&lt;should update, the new int value&gt;
		/// If the first item in the tuple is true, the value is updated, and true is returned.
		/// Note that <paramref name="updateIfTrue"/> may be called many times so it should be idempotent.
		/// </summary>
		/// <returns>The third value from the tuple return by <paramref name="updateIfTrue"/>.</returns>
		public static bool ConditionallySwap(ref int reference, Func<int, Tuple<bool, int>> updateIfTrue)
		{
			var spinWait = new SpinWait();
			while(true)
			{
				var current = reference;
				var t = updateIfTrue(current);
				if(!t.Item1) return false;
				if(CompareExchange(ref reference, current, t.Item2)) return true;
				spinWait.SpinOnce();
			}
		}

		/// <summary>
		/// Atomically updates the int <paramref name="reference"/> by calling <paramref name="updateIfTrue"/> to get the new value.
		/// <paramref name="updateIfTrue"/> returns a Tuple&lt;should update, the new int value&gt;
		/// If the first item in the tuple is true, the value is updated, and true is returned.
		/// Note that <paramref name="updateIfTrue"/> may be called many times so it should be idempotent.
		/// </summary>
		/// <returns><c>true</c> if the value was swapped.</returns>
		public static bool ConditionallySwap<T>(ref T reference, Func<T, Tuple<bool, T>> updateIfTrue) where T : class
		{
			var spinWait = new SpinWait();
			while(true)
			{
				var current = reference;
				var t = updateIfTrue(current);
				if(!t.Item1) return false;
				if(CompareExchange(ref reference, current, t.Item2)) return true;
				spinWait.SpinOnce();
			}
		}
		/// <summary>
		/// Atomically updates the <paramref name="reference"/> if <paramref name="shouldUpdate"/> returns true. 
		/// The new value is then produced by calling <paramref name="newValueCreator"/>.
		/// 
		/// Note that <paramref name="shouldUpdate"/> and <paramref name="newValueCreator"/> may be called many times so it should be idempotent.
		/// </summary>
		/// <returns><c>true</c> if the value was swapped.</returns>
		public static bool ConditionallySwap<T>(ref T reference, Func<T, bool> shouldUpdate, Func<T, T> newValueCreator) where T : class
		{
			var spinWait = new SpinWait();
			while(true)
			{
				var current = reference;
				if(!shouldUpdate(current)) return false;
				var newValue = newValueCreator(current);
				if(CompareExchange(ref reference, current, newValue)) return true;
				spinWait.SpinOnce();
			}
		}
		/// <summary>
		/// Atomically updates the <paramref name="reference"/> to <paramref name="newValue"/> if <paramref name="shouldUpdate"/> returns true. 
		/// 
		/// Note that <paramref name="shouldUpdate"/> may be called many times so it should be idempotent.
		/// </summary>
		/// <returns><c>true</c> if the value was swapped.</returns>
		public static bool ConditionallySwap<T>(ref T reference, Func<T, bool> shouldUpdate, T newValue) where T : class
		{
			var spinWait = new SpinWait();
			while(true)
			{
				var current = reference;
				if(!shouldUpdate(current)) return false;
				if(CompareExchange(ref reference, current, newValue)) return true;
				spinWait.SpinOnce();
			}
		}

		/// <summary>
		/// Atomically updates the int <paramref name="reference"/> by calling <paramref name="updateIfTrue"/> to get the new value.
		/// <paramref name="updateIfTrue"/> returns a Tuple&lt;should update, the new int value, the return value&gt;
		/// If the first item in the tuple is true, the value is updated, and the third value of the tuple is returned.
		/// Note that <paramref name="updateIfTrue"/> may be called many times so it should be idempotent.
		/// </summary>
		/// <returns>The third value from the tuple return by <paramref name="updateIfTrue"/>.</returns>
		public static TReturn ConditionallySwap<T, TReturn>(ref T reference, Func<T, Tuple<bool, T, TReturn>> updateIfTrue) where T : class
		{
			var spinWait = new SpinWait();
			while(true)
			{
				var current = reference;
				var t = updateIfTrue(current);
				if(!t.Item1) return t.Item3;
				if(CompareExchange(ref reference, current, t.Item2)) return t.Item3;
				spinWait.SpinOnce();
			}
		}

		/// <summary>
		/// Atomically updates the int <paramref name="reference"/> by calling <paramref name="updateIfTrue"/> to get the new value.
		/// <paramref name="updateIfTrue"/> returns a Tuple&lt;should update, the new int value, the return value&gt;
		/// If the first item in the tuple is true, the value is updated, and the third value of the tuple is returned.
		/// Note that <paramref name="updateIfTrue"/> may be called many times so it should be idempotent.
		/// </summary>
		/// <returns>The third value from the tuple return by <paramref name="updateIfTrue"/>.</returns>
		public static TReturn ConditionallySwap<TReturn>(ref int reference, Func<int, Tuple<bool, int, TReturn>> updateIfTrue)
		{
			var spinWait = new SpinWait();
			while(true)
			{
				var current = reference;
				var t = updateIfTrue(current);
				if(!t.Item1) return t.Item3;
				if(CompareExchange(ref reference, current, t.Item2)) return t.Item3;
				spinWait.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool CompareExchange<T>(ref T reference, T expectedValue, T newValue) where T : class
		{
			return Interlocked.CompareExchange(ref reference, newValue, expectedValue) == expectedValue;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool CompareExchange(ref int reference, int expectedValue, int newValue) 
		{
			return Interlocked.CompareExchange(ref reference, newValue, expectedValue) == expectedValue;
		}
	}
}