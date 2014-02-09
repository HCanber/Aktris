using Aktris.Internals;
using Aktris.Internals.SystemMessages;
using Aktris.Messages;

namespace Aktris.Dispatching
{
	public class DeadLetterMailbox : Mailbox
	{
		private readonly ActorRef _deadLettersActor;

		public DeadLetterMailbox(ActorRef deadLettersActor)
		{
			_deadLettersActor = deadLettersActor;
		}

		public void SetActor(InternalActorRef actor) {/* Intentionally left blank */}

		public void Enqueue(Envelope envelope)
		{
			if(!(envelope.Message is DeadLetterMessage))
			{
				_deadLettersActor.Send(new DeadLetterMessage(envelope.Message, envelope.Sender, envelope.Receiver), envelope.Sender);
			}
		}

		public void EnqueueSystemMessage(SystemMessageEnvelope envelope)
		{
			_deadLettersActor.Send(new DeadLetterMessage(envelope.Message, envelope.Sender, envelope.Receiver), envelope.Sender);
		}

		public void Suspend(InternalActorRef actor) {/* Intentionally left blank */}

		public void Resume(InternalActorRef actor) {/* Intentionally left blank */}

		public bool IsSuspended { get { return false; } }
		public bool IsClosed { get { return false; } }
		public void DetachActor(InternalActorRef actor) {/* Intentionally left blank */}
	}
}