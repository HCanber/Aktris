using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals.SystemMessages
{
	public sealed class SystemMessageEnvelope
	{
		public SystemMessageEnvelope([NotNull] ActorRef receiver, [NotNull] SystemMessage message, [NotNull] ActorRef sender)
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
		public SystemMessage Message { get; private set; }

		[NotNull]
		public ActorRef Sender { get; private set; }

		public override string ToString()
		{
			var message = Message;
			var messageStr = message.ToString();
			var messageType = message.GetType();
			var messageTypeStr = message.GetType().Name;
			var messageTypeName = messageType.Name;
			if(messageStr == messageTypeStr)
				return StringFormat.SafeFormat("<{2}> [{0}] -> [{1}]", Sender, Receiver, messageTypeName);
			return StringFormat.SafeFormat("<{2}> [{0}] -> [{1}]: {3}", Sender, Receiver, messageTypeName, messageStr);
		}
	}
}