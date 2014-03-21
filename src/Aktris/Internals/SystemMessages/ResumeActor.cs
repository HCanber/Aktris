using System;

namespace Aktris.Internals.SystemMessages
{
	public class ResumeActor : ExceptionSystemMessage
	{
		public ResumeActor(Exception causedByFailure)
			: base(causedByFailure)
		{
		}
	}
}