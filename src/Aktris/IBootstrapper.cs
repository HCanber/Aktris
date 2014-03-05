using System;
using Aktris.Dispatching;
using Aktris.Internals;
using Aktris.Settings;

namespace Aktris
{
	public interface IBootstrapper
	{
		IUniqueNameCreator UniqueNameCreator { get; }
		LocalActorRefFactory LocalActorRefFactory { get; }
		Func<ActorPath, ActorSystem, ActorRef> DeadLetterActorCreator { get; }
		Func<IActionScheduler, Mailbox> DefaultMailboxCreator { get;  }
		IActionScheduler ActionScheduler { get;  }
		ISettings Settings { get; }
		ActorSystem CreateSystem(string name);
	}
}