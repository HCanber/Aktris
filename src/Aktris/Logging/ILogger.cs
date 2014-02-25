using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Logging
{
	public interface ILogger
	{
		bool IsDebugEnabled { get; }
		bool IsInfoEnabled { get; }
		bool IsWarningEnabled { get; }
		bool IsErrorEnabled { get; }

		bool AreAnyEnabled(LogLevel levels);
		bool AreAllEnabled(LogLevel levels);
		LogLevel GetEnabledLevels(LogLevel levels);

		[StringFormatMethod("message")]
		void Debug(string message, params object[] args);

		[StringFormatMethod("message")]
		void Info(string message, params object[] args);

		[StringFormatMethod("message")]
		void Warning(string message, params object[] args);

		[StringFormatMethod("message")]
		void Error(string message, params object[] args);

		[StringFormatMethod("message")]
		void Error(Exception exception, string message, params object[] args);

		[StringFormatMethod("message")]
		void Log(LogLevel logLevel, string message, params object[] args);

	}
}