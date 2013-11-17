using Aktris.Internals;

namespace Aktris.Dispatching
{
	public interface Mailbox
	{
		void Attach(ILocalActorRef actor);
		void Enqueue(Envelope envelope);
	}
}