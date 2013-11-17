using Aktris.Internals;
using Aktris.Internals.SystemMessages;

namespace Aktris.Dispatching
{
	public interface Mailbox
	{
		void SetActor(ILocalActorRef actor);
		void Enqueue(Envelope envelope);
		void EnqueueSystemMessage(SystemMessageEnvelope envelope);
	}
}