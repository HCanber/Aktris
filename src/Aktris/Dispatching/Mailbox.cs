using Aktris.Internals;
using Aktris.Internals.SystemMessages;

namespace Aktris.Dispatching
{
	// ReSharper disable once InconsistentNaming
	public interface Mailbox
	{
		void SetActor(InternalActorRef actor);
		void Enqueue(Envelope envelope);
		void EnqueueSystemMessage(SystemMessageEnvelope envelope);
		void Suspend(InternalActorRef actor);
		void Resume(InternalActorRef actor);
		bool IsSuspended { get; }
		bool IsClosed { get; }
		void DetachActor(InternalActorRef actor);
	}
}