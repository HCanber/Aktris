using System;
using System.Threading.Tasks;

namespace Aktris.Internals.Concurrency
{
	public interface IPromise<T> : IDisposable
	{
		Task<T> Future { get; }
		bool IsCompleted { get; }
		bool TrySuccess(T result);
		bool TryFailure(Exception exception);
	}
}