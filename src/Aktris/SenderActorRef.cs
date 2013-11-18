using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris
{
	public class SenderActorRef : ActorRef
	{
		private readonly ActorRef _sender;
		private readonly ActorRef _instance;

		public SenderActorRef([NotNull] ActorRef sender, [NotNull] ActorRef instance)
		{
			if(sender == null) throw new ArgumentNullException("sender");
			if(instance == null) throw new ArgumentNullException("instance");
			_sender = sender;
			_instance = instance;
		}

		public string Name
		{
			get { return _sender.Name; }
		}

		void ActorRef.Send(object message, ActorRef sender)
		{
			_sender.Send(message, sender);
		}

		public void Reply(object message)
		{
			_sender.Send(message, _instance);
		}
	}
}