using System;

namespace Aktris.Internals.SystemMessages
{
	public class RecreateActor : ExceptionSystemMessage
	{
		private readonly Exception _causedByFailure;

		public RecreateActor(Exception causedByFailure)
		{
			_causedByFailure = causedByFailure;
		}

		public Exception CausedByFailure { get { return _causedByFailure; } }
	}
}