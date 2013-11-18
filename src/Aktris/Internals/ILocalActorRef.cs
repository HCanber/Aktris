using Aktris.Dispatching;
using Aktris.Internals.SystemMessages;

namespace Aktris.Internals
{
	public interface ILocalActorRef : ActorRef
	{
		void Start();
		void HandleMessage(Envelope envelope);
		void HandleSystemMessage(SystemMessageEnvelope envelope);
	}
}