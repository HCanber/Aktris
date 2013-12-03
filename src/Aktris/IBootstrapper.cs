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
		Func<ActorPath,ActorRef> DeadLetterActorCreator { get; }
		Func<Mailbox> DefaultMailboxCreator { get; }
	}
}