using System;
using System.Collections;
using System.Collections.Generic;

namespace Aktris.Internals.Helpers
{
	public class EmptyEnumerator<T> : IEnumerator<T>
	{
		private static readonly EmptyEnumerator<T> _Instance = new EmptyEnumerator<T>();
		private EmptyEnumerator() { }

		public T Current
		{
			get { throw new InvalidOperationException(); }
		}

		object IEnumerator.Current
		{
			get { throw new InvalidOperationException(); }
		}

		public static EmptyEnumerator<T> Instance { get { return _Instance; } }

		public bool MoveNext()
		{
			return false;
		}

		public void Reset()
		{
		}

		public void Dispose()
		{			
		}
	}
}