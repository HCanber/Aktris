using Aktris.Internals;

namespace Aktris.Dispatching
{
	public interface Mailbox
	{
		void SetActor(ILocalActorRef actor);
		void Enqueue(Envelope envelope);
	}
}