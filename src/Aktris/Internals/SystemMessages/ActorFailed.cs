using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals.SystemMessages
{
	public class ActorFailed : ExceptionSystemMessage
	{
		private readonly ActorRef _child;
		private readonly Exception _causedByFailure;

		public ActorFailed([NotNull] ActorRef child, [NotNull] Exception causedByFailure, uint instanceId)
		{
			if(child == null) throw new ArgumentNullException("child");
			if(causedByFailure == null) throw new ArgumentNullException("causedByFailure");
			_child = child;
			_causedByFailure = causedByFailure;
		}

		[NotNull]
		public ActorRef Child { get { return _child; } }

		[NotNull]
		public Exception CausedByFailure { get { return _causedByFailure; } }

		public override string ToString()
		{
			return "ActorFailed: [" + _child + "]. Cause: " + _causedByFailure;
		}
	}
}