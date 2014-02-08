﻿using System.Collections;
using System.Collections.Generic;

namespace Aktris.Internals.Helpers
{
	public class EmptyReadonlyCollection<T> : IReadOnlyCollection<T>
	{
		private static readonly EmptyReadonlyCollection<T> _Instance = new EmptyReadonlyCollection<T>();
		private EmptyReadonlyCollection() { }

		public IEnumerator<T> GetEnumerator()
		{
			return EmptyEnumerator<T>.Instance;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return EmptyEnumerator<T>.Instance;
		}

		public int Count { get { return 0; } }

		public static EmptyReadonlyCollection<T> Instance { get { return _Instance; } }
	}
}