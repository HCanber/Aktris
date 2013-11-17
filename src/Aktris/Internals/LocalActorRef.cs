using System;
using Aktris.Dispatching;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals
{
	public class LocalActorRef : ILocalActorRef
	{
		private readonly ActorInstantiator _actorInstantiator;
		private readonly string _name;
		private readonly Mailbox _mailbox;

		public LocalActorRef([NotNull] ActorInstantiator actorInstantiator, [NotNull] string name, [NotNull] Mailbox mailbox)
		{
			if(actorInstantiator == null) throw new ArgumentNullException("actorInstantiator");
			if(name == null) throw new ArgumentNullException("name");
			if(mailbox == null) throw new ArgumentNullException("mailbox");
			_actorInstantiator = actorInstantiator;
			_name = name;
			_mailbox = mailbox;
		}

		public string Name { get { return _name; } }

		public void Start()
		{
			_mailbox.Attach(this);
		}

		public void Send(object message, ActorRef sender)
		{
			var envelope=new Envelope(this, message, sender);
			_mailbox.Enqueue(envelope);
		}

		public void HandleMessage(Envelope envelope)
		{
			throw new NotImplementedException();
		}
	}
}