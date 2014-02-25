using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Logging
{
	public static class LoggerExtensions
	{
		public static void Error(this ILogger logger, Func<string> formatMessage)
		{
			if(!logger.IsErrorEnabled) return;
			Log(logger,formatMessage,m=>logger.Error(m));
		}

		public static void Warning(this ILogger logger, Func<string> formatMessage)
		{
			if(!logger.IsWarningEnabled) return;
			Log(logger, formatMessage, m => logger.Warning(m));
		}

		public static void Info(this ILogger logger, Func<string> formatMessage)
		{
			if(!logger.IsInfoEnabled) return;
			Log(logger, formatMessage, m => logger.Info(m));
		}

		public static void Debug(this ILogger logger, Func<string> formatMessage)
		{
			if(!logger.IsDebugEnabled) return;
			Log(logger, formatMessage, m => logger.Debug(m));
		}

		private static void Log(ILogger logger, Func<string> formatMessage, Action<string> log)
		{
			string message;
			try
			{
				message = formatMessage();
			}
			catch(Exception e)
			{
				logger.Error(e, "Error occurred while trying to format message");
				return;
			}
			log(message);
		}

	}
}