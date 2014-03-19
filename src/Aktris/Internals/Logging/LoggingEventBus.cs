﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Aktris.Events;
using Aktris.Exceptions;
using Aktris.Internals.Helpers;
using Aktris.JetBrainsAnnotations;
using Aktris.Logging;
using Aktris.Settings;

namespace Aktris.Internals.Logging
{
	public abstract class LoggingEventBus : ActorEventBus<LogEvent, Type>
	{
		private readonly ActorSystem _system;
		private readonly ActorRef _deadLetterActor;
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
		private List<ActorRef> _loggers = new List<ActorRef>();
		private LogLevel _logLevels = LogLevel.Error;


		protected LoggingEventBus([NotNull] ActorSystem system)
		{
			if(system == null) throw new ArgumentNullException("system");
			_system = system;
			_deadLetterActor = system.DeadLetters;
		}



		public LogLevel LogLevels
		{
			get { return _lock.Read(() => _logLevels); }
			set
			{
				if(value != _logLevels)
				{
					_lock.Write(() =>
					{
						if(value != _logLevels)
						{
							// subscribe if previously ignored and now requested
							var prevUnsubscribedThatShouldBeSubscribed = Log.GetSubscribeLevels(value).Where(l => !_logLevels.HasFlag(l.LogLevel));
							AllLoggersWhenInLock((a, t) => Subscribe(a, t), prevUnsubscribedThatShouldBeSubscribed);

							// unsubscribe if previously registered and now ignored
							var prevSubscribedThatShouldBeUnsubscribed = Log.GetSubscribeLevels(_logLevels).Where(l => !_logLevels.HasFlag(l.LogLevel));
							AllLoggersWhenInLock((subscriber, to) => Unsubscribe(subscriber, to), prevSubscribedThatShouldBeUnsubscribed);
							_logLevels = value;
						}
					});
				}
			}
		}

		private void AllLoggersWhenInLock(Action<ActorRef, Type> action, IEnumerable<LogLevelWithType> levels)
		{
			foreach(var level in levels)
			{
				foreach(var logger in _loggers)
				{
					action(logger, level);
				}
			}
		}


		private void AddLogger(ActorRef logger, LogLevel logLevels, string logName)
		{
			logger.Send(new InitializeLogger(this), null);
			Log.SeparateLogLevelsToSequence(logLevels).ForEach(l => Subscribe(logger, l.Type));
			Publish(new DebugLogEvent(logName, GetType(), "Logger " + logName + " started"));
		}

		public abstract bool Subscribe(ActorRef subscriber, Type to);

		public abstract bool Unsubscribe(ActorRef subscriber);

		public abstract bool Unsubscribe(ActorRef subscriber, Type to);

		public abstract void Publish(object @event);




		public void StartStandardOutLogger(StandardOutLogger standardOutLogger, IStandardOutLoggerSettings settings)
		{
			SetupStandardOutLogger(standardOutLogger, settings);
			Publish(new DebugLogEvent(GetType().Name, GetType(), "StandardOutLogger started"));

		}

		private void SetupStandardOutLogger(StandardOutLogger stdOutLogger, IStandardOutLoggerSettings settings)
		{
			LogLevel logLevel = settings.LogLevel;
			var dateFormat = settings.DateFormat;
			if(!Log.IsValidLogLevel(logLevel))
			{
				StandardOutLoggerHelper.PrintError(new ErrorLogEvent(GetType().Name, GetType(), string.Format("Unknown StandardOutLogger LogLevel setting: {0}, defaulting to Error", logLevel), new LoggingException()), dateFormat);
				logLevel = LogLevel.Error;
			}
			Log.GetSubscribeLevels(logLevel).ForEach(level => Subscribe(stdOutLogger, level.Type));
			_lock.Write(() =>
			{
				_loggers.Add(stdOutLogger);
				_logLevels = logLevel;
			});
		}

		//TODO: stopDefaultLoggers

		public void StartDefaultLoggers()
		{
			var logName = GetType().Name + "(" + _system.Name + ")";
			var logLevels = Log.GetSubscribeLevels(_system.Settings.LogLevel).Aggregate(LogLevel.Off, (v, l) => v | l);
			var allLoggers = _system.Settings.Loggers ?? new List<Type> { typeof(DefaultLogger) };
			var loggerTypes = allLoggers.Where(t => t != typeof(StandardOutLogger));

			var loggers = new List<ActorRef>();
			foreach(var loggerType in loggerTypes)
			{
				var logger = _system.CreateInstance<ActorRef>(loggerType);
				AddLogger(logger, logLevels, logName);
				loggers.Add(logger);
			}
			_lock.Write(() =>
			{
				_loggers = loggers;
				_logLevels = logLevels;
			});

		}
	}

}