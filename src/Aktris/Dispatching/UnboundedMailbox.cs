using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Aktris.Internals;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Dispatching
{
	public class UnboundedMailbox : SchedulerBasedMailbox
	{
		private readonly IMailboxDeliveryStrategy _mailboxDeliveryStrategy;
		private readonly ConcurrentQueue<Envelope> _queue = new ConcurrentQueue<Envelope>();

		public UnboundedMailbox([NotNull] IScheduler scheduler)
			: this(scheduler, new SingleActorPerMailboxStrategy())
		{
		}

		public UnboundedMailbox([NotNull] IScheduler scheduler, [NotNull] IMailboxDeliveryStrategy mailboxDeliveryStrategy)
			: base(scheduler)
		{
			_mailboxDeliveryStrategy = mailboxDeliveryStrategy;
			if(scheduler == null) throw new ArgumentNullException("scheduler");
			if(mailboxDeliveryStrategy == null) throw new ArgumentNullException("mailboxDeliveryStrategy");
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

		protected override IEnumerable<ILocalActorRef> GetRecipients(Envelope envelope)
		{
			return _mailboxDeliveryStrategy.GetRecipient(envelope);
		}

		protected override void Register(ILocalActorRef actor)
		{
			_mailboxDeliveryStrategy.Register(actor);
		}
	}

	public class SingleActorPerMailboxStrategy : IMailboxDeliveryStrategy
	{
		private ILocalActorRef _actor;

		public void Register(ILocalActorRef actor)
		{
			var existingActor = Interlocked.CompareExchange(ref _actor, actor, null);
			if(existingActor != null)
			{
				throw new InvalidOperationException(StringFormat.SafeFormat("Cannot register more than on actor. Existing actor: {0}. Trying to register: {1}", existingActor, actor));
			}
		}

		public void Unregister(ILocalActorRef actor)
		{
			_actor = null;
		}

		public IEnumerable<ILocalActorRef> GetRecipient(Envelope envelope)
		{
			return new[] { _actor };
		}
	}

	public interface IMailboxDeliveryStrategy
	{
		void Register(ILocalActorRef actor);
		void Unregister(ILocalActorRef actor);
		IEnumerable<ILocalActorRef> GetRecipient(Envelope envelope);
	}
}