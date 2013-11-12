using System.Threading;

namespace Aktris.Internals
{
	public class RandomNameCreator : IRandomNameCreator
	{
		private static long _nextRandomNameNumber;

		public string GetNextRandomName()
		{
			var nextNumber = Interlocked.Increment(ref _nextRandomNameNumber);
			var tempName = Base64Helper.Encode(nextNumber);
			return tempName;
		}
 
	}
}