using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals.SystemMessages
{
	public class ActorFailed : SystemMessage
	{
		private readonly ActorRef _child;
		private readonly Exception _cause;

		public ActorFailed([NotNull] ActorRef child, Exception cause)
		{
			if(child == null) throw new ArgumentNullException("child");
			_child = child;
			_cause = cause;
		}

		[NotNull]
		public ActorRef Child { get { return _child; } }

		[CanBeNull]
		public Exception Cause { get { return _cause; } }
	}
}