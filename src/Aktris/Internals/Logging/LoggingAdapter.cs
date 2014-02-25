using System;
using Aktris.JetBrainsAnnotations;
using Aktris.Logging;

namespace Aktris.Internals.Logging
{
	public class LoggingAdapter : ILogger
	{
		private readonly LoggingEventBus _loggingEventBus;
		private readonly object _logClass;
		private string _logSource;

		public LoggingAdapter(LoggingEventBus loggingEventBus, object logClass)
			: this(loggingEventBus, logClass, LogSource.GetLogSource(logClass))
		{
		}

		public LoggingAdapter([NotNull] LoggingEventBus loggingEventBus, [NotNull] object logClass, [NotNull] string logSource)
		{
			if(loggingEventBus == null) throw new ArgumentNullException("loggingEventBus");
			if(logClass == null) throw new ArgumentNullException("logClass");
			if(logSource == null) throw new ArgumentNullException("logSource");
			if(logSource.Length == 0) throw new ArgumentException("Log source must be specified", "logSource");
			_loggingEventBus = loggingEventBus;
			_logClass = logClass;
			_logSource = logSource;
		}

		public bool IsDebugEnabled { get { return _loggingEventBus.LogLevels.HasFlag(LogLevel.Debug); } }

		public bool IsWarningEnabled { get { return _loggingEventBus.LogLevels.HasFlag(LogLevel.Warning); } }

		public bool IsInfoEnabled { get { return _loggingEventBus.LogLevels.HasFlag(LogLevel.Info); } }

		public bool IsErrorEnabled { get { return _loggingEventBus.LogLevels.HasFlag(LogLevel.Error); } }

		public bool AreAnyEnabled(LogLevel levels)
		{
			return (_loggingEventBus.LogLevels & levels) != LogLevel.Off;
		}

		public bool AreAllEnabled(LogLevel levels)
		{
			return (_loggingEventBus.LogLevels & levels) == levels;
		}

		public LogLevel GetEnabledLevels(LogLevel levels)
		{
			return _loggingEventBus.LogLevels & levels;
		}


		[StringFormatMethod("message")]
		public void Debug(string message, params object[] args)
		{
			if(IsDebugEnabled)
				_loggingEventBus.LogDebug(_logSource, _logClass, message, args);
		}

		[StringFormatMethod("message")]
		public void Info(string message, params object[] args)
		{
			if(IsInfoEnabled)
				_loggingEventBus.LogInfo(_logSource, _logClass, message, args);
		}

		[StringFormatMethod("message")]
		public void Warning(string message, params object[] args)
		{
			if(IsWarningEnabled)
				_loggingEventBus.LogWarning(_logSource, _logClass, message, args);

		}

		[StringFormatMethod("message")]
		public void Error(string message, params object[] args)
		{
			if(IsErrorEnabled)
				_loggingEventBus.LogError(_logSource, _logClass, message, args);
			StringFormat.SafeFormat(message, args);
		}

		[StringFormatMethod("message")]
		public void Error(Exception exception, string message, params object[] args)
		{
			if(IsErrorEnabled)
				_loggingEventBus.LogErrorException(_logSource, _logClass, exception, message, args);
		}

		[StringFormatMethod("message")]
		public void Log(LogLevel logLevels, string message, params object[] args)
		{
			if(logLevels == LogLevel.Off) return;
			var logLevelSequence = Logging.Log.SeparateLogLevelsToSequence(logLevels);
			foreach(var logLevel in logLevelSequence)
			{
				switch(logLevel)
				{
					case LogLevel.Error:
						Debug(message, args);
						break;
					case LogLevel.Warning:
						Warning(message, args);
						break;
					case LogLevel.Info:
						Info(message, args);
						break;
					case LogLevel.Debug:
						Debug(message, args);
						break;
				}
			}
		}
	}

	public static class LogSource
	{
		public static string GetLogSource(object instance)
		{
			return PatternMatcher<string>.Match<ActorRef>(instance, a => GetActorRefLogSource(a))
						 ?? PatternMatcher<string>.Match<Actor>(instance, a => GetActorRefLogSource(a.Self))
						 ?? instance.ToString();
		}

		private static string GetActorRefLogSource(ActorRef actorRef)
		{
			return actorRef.Path.ToString();	//TODO: Include systems address?
		}
	}
}