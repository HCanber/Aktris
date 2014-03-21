using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals.SystemMessages
{
	public class ActorFailed : ExceptionSystemMessage
	{
		private readonly ActorRef _child;

		public ActorFailed([NotNull] ActorRef child, [NotNull] Exception causedByFailure, uint instanceId)
			: base(causedByFailure)
		{
			if(child == null) throw new ArgumentNullException("child");
			if(causedByFailure == null) throw new ArgumentNullException("causedByFailure");
			_child = child;
		}

		[NotNull]
		public ActorRef Child { get { return _child; } }

		[NotNull]
		public new Exception CausedByFailure { get { return base.CausedByFailure; } }

		public override string ToString()
		{
			return "Child: [" + _child + "]" + CauseToString(". Cause: ");
		}
	}
}