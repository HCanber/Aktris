﻿using System;
using Aktris.Dispatching;
using Aktris.Internals.SystemMessages;

namespace Aktris.Internals
{
	public abstract class MinimalActorRef : InternalActorRef
	{
		public abstract string Name { get; }
		public virtual void Send(object message, ActorRef sender) {/* Intentionally left blank */}
		public virtual void Start() {/* Intentionally left blank */}
		public virtual void Stop() {/* Intentionally left blank */}
		public virtual void HandleMessage(Envelope envelope) {/* Intentionally left blank */}
		public virtual void HandleSystemMessage(SystemMessageEnvelope envelope) {/* Intentionally left blank */}
		public abstract ActorRef CreateActor(ActorCreationProperties actorCreationProperties, string name = null);
		public Mailbox Mailbox { get { return null; } }
		public virtual void Suspend() {/* Intentionally left blank */}
		public virtual void Resume(Exception causedByFailure) {/* Intentionally left blank */}
		public virtual void Restart(Exception causedByFailure) {/* Intentionally left blank */}
		public abstract ActorPath Path { get; }
		public abstract uint InstanceId { get; }
		public virtual void SendSystemMessage(SystemMessage message, ActorRef sender) {/* Intentionally left blank */}
	}
}