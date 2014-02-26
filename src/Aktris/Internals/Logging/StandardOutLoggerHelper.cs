using System;
using Aktris.Logging;

namespace Aktris.Internals.Logging
{
	public static class StandardOutLoggerHelper
	{
		public const string DefaultDateFormat = LineLogFormatter.DefaultDateFormat;

		public static void PrintError(ErrorLogEvent e, string dateFormat = DefaultDateFormat)
		{
			var msg = LineLogFormatter.FormatError(e, dateFormat);
			WriteLine(msg, LogLevel.Error);
		}

		public static void PrintWarning(WarningLogEvent e, string dateFormat = DefaultDateFormat)
		{
			var msg = LineLogFormatter.FormatWarning(e, dateFormat);
			WriteLine(msg, LogLevel.Warning);
		}

		public static void PrintInfo(InfoLogEvent e, string dateFormat = DefaultDateFormat)
		{
			var msg = LineLogFormatter.FormatInfo(e, dateFormat);
			WriteLine(msg, LogLevel.Info);
		}

		public static void PrintDebug(DebugLogEvent e, string dateFormat = DefaultDateFormat)
		{
			var msg = LineLogFormatter.FormatDebug(e, dateFormat);
			WriteLine(msg, LogLevel.Debug);
		}

		public static void Print(object message, string dateFormat = DefaultDateFormat)
		{
			if(message == null) return;
			// ReSharper disable ConvertClosureToMethodGroup
			var ignored = PatternMatcher.Match<ErrorLogEvent>(message, m => PrintError(m, dateFormat))
										|| PatternMatcher.Match<WarningLogEvent>(message, m => PrintWarning(m, dateFormat))
										|| PatternMatcher.Match<InfoLogEvent>(message, m => PrintInfo(m, dateFormat))
										|| PatternMatcher.Match<DebugLogEvent>(message, m => PrintDebug(m, dateFormat))
										|| PatternMatcher.MatchAll(() => PrintWarning(new WarningLogEvent(typeof(StandardOutLoggerHelper).Name, typeof(StandardOutLoggerHelper), "Received unexpected message of type " + message.GetType() + ": " + message)));
			// ReSharper restore ConvertClosureToMethodGroup
		}

		private static void WriteLine(string msg, LogLevel logLevel)
		{
			ConsoleColor foregroundColor;
			switch(logLevel)
			{
				case LogLevel.Error:
					foregroundColor = ConsoleColor.Red;
					break;
				case LogLevel.Warning:
					foregroundColor = ConsoleColor.Magenta;
					break;
				case LogLevel.Info:
					foregroundColor = ConsoleColor.White;
					break;
				case LogLevel.Debug:
					foregroundColor = ConsoleColor.Gray;
					break;
				default:
					foregroundColor = Console.ForegroundColor;
					break;
			}
			ConcurrentConsoleWriter.WriteLine(msg, foregroundColor);
		}
	}
}