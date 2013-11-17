using System;
using Aktris.Exceptions;
using Aktris.Internals;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Dispatching
{
	public sealed class Envelope
	{
		public Envelope([NotNull] object message, [NotNull] ActorRef sender)
		{
			if(message == null) throw new ArgumentNullException("message");
			//if(sender == null) throw new ArgumentNullException("sender");		//TODO: Remove this when we have DeadLetterMailbox
			Message = message;
			Sender = sender;
		}

		[NotNull]
		public object Message { get; private set; }

		[NotNull]
		public ActorRef Sender { get; private set; }

		public override string ToString()
		{
			return StringFormat.SafeFormat("From {0}: {1}", Sender, Message);
		}
	}
}