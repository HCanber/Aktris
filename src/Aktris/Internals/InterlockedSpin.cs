using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Aktris.Internals
{
	public static class InterlockedSpin
	{
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

		public static TReturn BreakableSwap<T, TReturn>(ref T reference, Func<T, Tuple<bool, T, TReturn>> breakIfTrueUpdateOtherwise) where T : class
		{
			var spinWait = new SpinWait();
			while(true)
			{
				var current = reference;
				var t = breakIfTrueUpdateOtherwise(current);
				if(t.Item1) return t.Item3;
				if(CompareExchange(ref reference, current, t.Item2)) return t.Item3;
				spinWait.SpinOnce();
			}
		}
		public static TReturn BreakableSwap<TReturn>(ref int reference, Func<int, Tuple<bool, int, TReturn>> breakIfTrueUpdateOtherwise)
		{
			var spinWait = new SpinWait();
			while(true)
			{
				var current = reference;
				var t = breakIfTrueUpdateOtherwise(current);
				if(t.Item1) return t.Item3;
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