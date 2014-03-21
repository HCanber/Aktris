using System;

namespace Aktris.Internals.SystemMessages
{
	public class RecreateActor : ExceptionSystemMessage
	{
		public RecreateActor(Exception causedByFailure)
			: base(causedByFailure)
		{
		}
	}
}