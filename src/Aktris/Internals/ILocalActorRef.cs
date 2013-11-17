using Aktris.Dispatching;

namespace Aktris.Internals
{
	public interface ILocalActorRef : ActorRef
	{
		void Start();
		void HandleMessage(Envelope envelope);
	}
}