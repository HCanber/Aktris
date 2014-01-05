using System;

namespace Aktris.Internals.SystemMessages
{
	// ReSharper disable once InconsistentNaming
	public interface ExceptionSystemMessage : SystemMessage
	{
		Exception CausedByFailure { get; }
	}
}