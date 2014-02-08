using System;
using System.Threading;

namespace Aktris.Internals
{
	public static class ReaderWriterLockSlimExtension
	{
		public static void Read(this ReaderWriterLockSlim lockSlim, Action action)
		{
			try
			{
				lockSlim.EnterReadLock();
				action();
			}
			finally
			{
				lockSlim.ExitReadLock();
			}
		}

		public static T Read<T>(this ReaderWriterLockSlim lockSlim, Func<T> read)
		{
			try
			{
				lockSlim.EnterReadLock();
				return read();
			}
			finally
			{
				lockSlim.ExitReadLock();
			}
		}

		public static void Write(this ReaderWriterLockSlim lockSlim, Action action)
		{
			try
			{
				lockSlim.EnterWriteLock();
				action();
			}
			finally
			{
				lockSlim.ExitWriteLock();
			}
		}
	}
}