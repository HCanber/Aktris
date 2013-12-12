using System;

namespace Aktris.Internals.SystemMessages
{
	public class RestartActor : SystemMessage
	{
		private readonly Exception _causedByFailure;

		public RestartActor(Exception causedByFailure)
		{
			_causedByFailure = causedByFailure;
		}

		public Exception CausedByFailure { get { return _causedByFailure; } }
	}
}