using System;
using Aktris.Exceptions;
using Aktris.Internals;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Dispatching
{
	public sealed class Envelope
	{
		public Envelope([NotNull] ActorRef receiver, [NotNull] object message, [NotNull] ActorRef sender)
		{
			if(receiver == null) throw new ArgumentNullException("receiver");
			if(message == null) throw new ArgumentNullException("message");
			//if(sender == null) throw new ArgumentNullException("sender");		//TODO: Remove this when we have DeadLetterMailbox
			Receiver = receiver;
			Message = message;
			Sender = sender;
		}

		public ActorRef Receiver { get; private set; }

		[NotNull]
		public object Message { get; private set; }

		[NotNull]
		public ActorRef Sender { get; private set; }

		public override string ToString()
		{
			return StringFormat.SafeFormat("From [{0}] to [{1}]: {2}", Sender, Receiver, Message);
		}
	}
}