using System;
using System.Threading;

namespace Aktris.Internals.Helpers
{
	/// <summary>
	/// Provides a instance of <see cref="Random"/> per thread, since <see cref="Random"/> is not thread safe.
	/// </summary>
	public static class RandomProvider
	{
		//Thanks Jon Skeet for this. http://csharpindepth.com/Articles/Chapter12/Random.aspx
		private static int _seed = Environment.TickCount;

		private static readonly ThreadLocal<Random> _RandomWrapper = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

		public static Random GetThreadRandom()
		{
			return _RandomWrapper.Value;
		}
	}
}