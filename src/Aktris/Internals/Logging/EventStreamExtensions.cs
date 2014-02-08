using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals.Logging
{
	public static class EventStreamExtensions
	{
		[StringFormatMethod("message")]
		public static void LogDebug(this EventStream eventStream, string logSource, object logClass, string message, params object[] args)
		{
			eventStream.Publish(new DebugLogEvent(logSource, logClass.GetType(), StringFormat.SafeFormat(message, args)));
		}

		[StringFormatMethod("message")]
		public static void LogInfo(this EventStream eventStream, string logSource, object logClass, string message, params object[] args)
		{
			eventStream.Publish(new InfoLogEvent(logSource, logClass.GetType(), StringFormat.SafeFormat(message, args)));
		}

		[StringFormatMethod("message")]
		public static void LogWarning(this EventStream eventStream, string logSource, object logClass, string message, params object[] args)
		{
			eventStream.Publish(new WarningLogEvent(logSource, logClass.GetType(), StringFormat.SafeFormat(message, args)));
		}

		[StringFormatMethod("message")]
		public static void LogError(this EventStream eventStream, string logSource, object logClass, string message, params object[] args)
		{
			eventStream.Publish(new ErrorLogEvent(logSource, logClass.GetType(), StringFormat.SafeFormat(message, args)));
		}
		[StringFormatMethod("message")]
		public static void LogErrorException(this EventStream eventStream, string logSource, object logClass, Exception exception, string message, params object[] args)
		{
			eventStream.Publish(new ErrorLogEvent(logSource, logClass.GetType(), StringFormat.SafeFormat(message, args), exception));
		}
	}
}