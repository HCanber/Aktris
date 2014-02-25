using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals.Logging
{
	public static class LoggingEventBusExtensions
	{
		[StringFormatMethod("message")]
		public static void LogDebug(this LoggingEventBus loggingEventBus, string logSource, object logClass, string message, params object[] args)
		{
			loggingEventBus.Publish(new DebugLogEvent(logSource, logClass.GetType(), StringFormat.SafeFormat(message, args)));
		}

		[StringFormatMethod("message")]
		public static void LogInfo(this LoggingEventBus loggingEventBus, string logSource, object logClass, string message, params object[] args)
		{
			loggingEventBus.Publish(new InfoLogEvent(logSource, logClass.GetType(), StringFormat.SafeFormat(message, args)));
		}

		[StringFormatMethod("message")]
		public static void LogWarning(this LoggingEventBus loggingEventBus, string logSource, object logClass, string message, params object[] args)
		{
			loggingEventBus.Publish(new WarningLogEvent(logSource, logClass.GetType(), StringFormat.SafeFormat(message, args)));
		}

		[StringFormatMethod("message")]
		public static void LogError(this LoggingEventBus loggingEventBus, string logSource, object logClass, string message, params object[] args)
		{
			loggingEventBus.Publish(new ErrorLogEvent(logSource, logClass.GetType(), StringFormat.SafeFormat(message, args)));
		}

		[StringFormatMethod("message")]
		public static void LogErrorException(this LoggingEventBus loggingEventBus, string logSource, object logClass, Exception exception, string message, params object[] args)
		{
			loggingEventBus.Publish(new ErrorLogEvent(logSource, logClass.GetType(), StringFormat.SafeFormat(message, args), exception));
		}
	}
}