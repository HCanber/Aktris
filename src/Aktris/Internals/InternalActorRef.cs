using System;
using Aktris.Dispatching;
using Aktris.Internals.Children;
using Aktris.Internals.SystemMessages;

namespace Aktris.Internals
{
// ReSharper disable once InconsistentNaming
	public interface InternalActorRef : ActorRef, InternalMessageHandler
	{
		void Start();
		ActorRef CreateActor(ActorCreationProperties actorCreationProperties, string name = null);
		uint InstanceId { get; }
		Mailbox Mailbox { get; }
		bool IsTerminated { get; }
		void Suspend();
		void Resume(Exception causedByFailure);
		void SendSystemMessage(SystemMessage message, ActorRef sender);
		void Restart(Exception causedByFailure);
		void UnwatchAndStopChildren();
		void Watch(InternalActorRef actorToWatch);
		void Unwatch(InternalActorRef actorToWatch);
		void Stop();
	}
}