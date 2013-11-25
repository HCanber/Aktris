using System;
using Aktris.Dispatching;
using Aktris.JetBrainsAnnotations;

namespace Aktris
{
	public class DelegateActorCreationProperties : ActorCreationProperties
	{
		private readonly Func<Actor> _factory;

		public DelegateActorCreationProperties([NotNull] Func<Actor> factory)
		{
			if(factory == null) throw new ArgumentNullException("factory");
			_factory = factory;
			MailboxCreator = null;
		}

		public Func<Mailbox> MailboxCreator { get; set; }
		public override Mailbox CreateMailbox()
		{
			return MailboxCreator == null ? null : MailboxCreator();
		}

		public override Actor CreateNewActor()
		{
			return _factory();
		}
	}
}