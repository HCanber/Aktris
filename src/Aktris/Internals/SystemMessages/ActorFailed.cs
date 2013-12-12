using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals.SystemMessages
{
	public class ActorFailed : SystemMessage
	{
		private readonly ActorRef _child;
		private readonly Exception _cause;

		public ActorFailed([NotNull] ActorRef child, [NotNull] Exception cause, uint instanceId)
		{
			if(child == null) throw new ArgumentNullException("child");
			if(cause == null) throw new ArgumentNullException("cause");
			_child = child;
			_cause = cause;
		}

		[NotNull]
		public ActorRef Child { get { return _child; } }

		[NotNull]
		public Exception Cause { get { return _cause; } }
	}
}