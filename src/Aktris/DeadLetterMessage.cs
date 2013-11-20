using System;
using Aktris.Internals;
using Aktris.JetBrainsAnnotations;

namespace Aktris
{
	public class DeadLetterMessage
	{
		private readonly object _message;
		private readonly ActorRef _sender;
		private readonly ActorRef _recipient;

		public DeadLetterMessage(object message, [NotNull] ActorRef sender, [NotNull] ActorRef recipient)
		{
			if(sender == null) throw new ArgumentNullException("sender");
			if(recipient == null) throw new ArgumentNullException("recipient");
			_message = message;
			_sender = sender;
			_recipient = recipient;
		}

		public object Message { get { return _message; } }

		public ActorRef Sender { get { return _sender; } }

		public ActorRef Recipient { get { return _recipient; } }

		public override string ToString()
		{
			return StringFormat.SafeFormat("DeadLetter from [{0}] to [{1}]: {2}", Sender, Recipient, Message);
		}
	}
}