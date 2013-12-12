using System;
using Aktris.Dispatching;
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
		void Suspend();
		void Resume(Exception causedByFailure);
		void SendSystemMessage(SystemMessage message, ActorRef sender);
		void Stop();
		void Restart(Exception causedByFailure);
	}
}