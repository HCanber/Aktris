using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Aktris.Internals;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Dispatching
{
	public class UnboundedMailbox : SchedulerBasedMailbox, IFirstQueueable<Envelope>
	{
		private readonly ConcurrentQueue<Envelope> _queue = new ConcurrentQueue<Envelope>();
		private readonly ConcurrentStack<Envelope> _first = new ConcurrentStack<Envelope>();

		public UnboundedMailbox([NotNull] IActionScheduler scheduler)
			: base(scheduler)
		{
			if(scheduler == null) throw new ArgumentNullException("scheduler");
		}

		protected override bool TryGetMessageToProcess(out Envelope message)
		{
			if(!_first.IsEmpty)
			{
				if(_first.TryPop(out message))
					return true;
			}
			return _queue.TryDequeue(out message);
		}

		protected override IEnumerable<Envelope> GetMessagesToProcess()
		{
			Envelope envelope;
			while(true)
			{
				if(_first.TryPop(out envelope))
				{					
					yield return envelope;
				}
				else if(_queue.TryDequeue(out envelope))
				{
					yield return envelope;
				}
				else
				{
					yield break;
				}
			}
		}

		protected override void InternalEnqueue(Envelope envelope)
		{
			_queue.Enqueue(envelope);
		}

		public void EnqueueFirst(IReadOnlyList<Envelope> envelopes)
		{
			//Push them in reverse order, since we put them on the stack, we want the envelopes[0] message to be pushed last, so that it will be popped first.
			var count = envelopes.Count;
			for(var i = count - 1; i >= 0; i--)
			{
				_first.Push(envelopes[i]);
			}
		}

		protected override bool HasMessagesEnqued()
		{
			return _queue.Count > 0;
		}

	}
}