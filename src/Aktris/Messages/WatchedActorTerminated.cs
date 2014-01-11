using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Messages
{
	public class WatchedActorTerminated
	{
		private readonly ActorRef _terminatedActor;

		public WatchedActorTerminated([NotNull] ActorRef terminatedActor)
		{
			if(terminatedActor == null) throw new ArgumentNullException("terminatedActor");
			_terminatedActor = terminatedActor;
		}

		[NotNull]
		public ActorRef TerminatedActor { get { return _terminatedActor; } }
	}
}