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