﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Dispatching
{
	public class UnboundedMailbox : SchedulerBasedMailbox
	{
		private readonly ConcurrentQueue<Envelope> _queue = new ConcurrentQueue<Envelope>();

		public UnboundedMailbox([NotNull] IScheduler scheduler):base(scheduler)
		{
			if(scheduler == null) throw new ArgumentNullException("scheduler");
		}

		protected override IEnumerable<Envelope> GetMessagesToProcess()
		{
			Envelope envelope;
			while(_queue.TryDequeue(out envelope))
			{
				yield return envelope;
			}
		}

		protected override void InternalEnqueue(Envelope envelope)
		{
			_queue.Enqueue(envelope);
		}

		protected override bool HasMessagesEnqued()
		{
			return _queue.Count > 0;
		}
	}
}