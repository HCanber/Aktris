using System;
using Aktris.Internals.Logging;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals.SystemMessages
{
	public abstract class ExceptionSystemMessage : SystemMessage
	{
		private readonly Exception _causedByFailure;

		protected ExceptionSystemMessage(Exception causedByFailure)
		{
			_causedByFailure = causedByFailure;
		}

		public Exception CausedByFailure { get { return _causedByFailure; } }

		public override string ToString()
		{
			return CauseToString();
		}

		protected string CauseToString(string prefix = "Cause: ")
		{
			if(_causedByFailure == null) return "";
			return ExceptionFormatter.DebugFormat(_causedByFailure, prefix);
		}
	}
}