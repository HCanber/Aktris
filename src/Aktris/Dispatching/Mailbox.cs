using System.Threading;
using Aktris.Internals;

namespace Aktris.Dispatching
{
	public interface Mailbox
	{
		void Attach(ILocalActorRef actor);
	}

	public class MailboxBase : Mailbox
	{
		private long _numberOfAttachedActors = 0;

		public long NumberOfAttachedActors { get { return _numberOfAttachedActors; } }

		public void Attach(ILocalActorRef actor)
		{
			Register(actor);
		}

		protected virtual void Register(ILocalActorRef actor)
		{
			Interlocked.Increment(ref _numberOfAttachedActors);
		}
	}

	public class UnboundedMailbox : MailboxBase
	{
	}
}