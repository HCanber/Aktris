using System.Collections.Generic;

namespace Aktris.Dispatching
{
	public interface IFirstQueueable<in T>
	{
		void EnqueueFirst(IReadOnlyList<T> items);
	}
}