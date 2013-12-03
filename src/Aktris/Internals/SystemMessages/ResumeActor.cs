using System;

namespace Aktris.Internals.SystemMessages
{
	public class ResumeActor : SystemMessage
	{
		private readonly Exception _causedByFailure;

		public ResumeActor(Exception causedByFailure)
		{
			_causedByFailure = causedByFailure;
		}

		public Exception CausedByFailure { get { return _causedByFailure; } }
	}
}