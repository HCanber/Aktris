using Aktris.Dispatching;
using Aktris.Internals.SystemMessages;

namespace Aktris.Internals
{
// ReSharper disable once InconsistentNaming
	public interface InternalActorRef : ActorRef
	{
		void Start();
		void HandleMessage(Envelope envelope);
		void HandleSystemMessage(SystemMessageEnvelope envelope);
		ActorRef CreateActor(ActorCreationProperties actorCreationProperties, string name = null);
		uint InstanceId { get; }
	}
}