using System;
using Aktris.Dispatching;
using Aktris.Internals;
using Fibrous.Experimental.Actors;

namespace Aktris
{
	public interface IBootstrapper
	{
		IUniqueNameCreator UniqueNameCreator { get; }
		LocalActorRefFactory LocalActorRefFactory { get; }
		Func<ActorPath, ActorSystem, ActorRef> DeadLetterActorCreator { get; }
		Func<IScheduler, Mailbox> DefaultMailboxCreator { get;  }
		IScheduler Scheduler { get;  }
	}
}