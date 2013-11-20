using System;
using Aktris.Internals;
using Fibrous.Experimental.Actors;

namespace Aktris
{
	public interface IBootstrapper
	{
		IUniqueNameCreator UniqueNameCreator { get; }
		LocalActorRefFactory LocalActorRefFactory { get; }
		Func<ActorRef> DeadLetterActorCreator { get; }
	}
}