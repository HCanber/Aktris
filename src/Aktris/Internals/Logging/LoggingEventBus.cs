using System;
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
		private readonly ActorRef _deadLetterActor;
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
		private readonly List<ActorRef> _loggers = new List<ActorRef>();
		private LogLevel _logLevel = LogLevel.Error;


		protected LoggingEventBus([NotNull] ActorRef deadLetterActor)
		{
			if(deadLetterActor == null) throw new ArgumentNullException("deadLetterActor");
			_deadLetterActor = deadLetterActor;
		}

		public LogLevel LogLevel
		{
			get { return _lock.Read(() => _logLevel); }
			set
			{
				_lock.Write(() =>
				{
					if(value > _logLevel)
					{
						// subscribe if previously ignored and now requested
						var prevUnsubscribedThatShouldBeSubscribed = Log.GetSubscribeLevels(value).Where(l => l > _logLevel);
						AllLoggersWhenInLock((a,t)=>Subscribe(a,t), prevUnsubscribedThatShouldBeSubscribed);						
					}
					else if(value<_logLevel)
					{
						// unsubscribe if previously registered and now ignored
						var prevSubscribedThatShouldBeUnsubscribed = Log.GetSubscribeLevels(_logLevel).Where(l => l > value);
						AllLoggersWhenInLock((subscriber, to) => Unsubscribe(subscriber, to),prevSubscribedThatShouldBeUnsubscribed);
					}
					_logLevel = value;
				});
			}
		}

		private void AllLoggersWhenInLock(Action<ActorRef,Type> action, IEnumerable<LogLevelWithType> levels)
		{
			foreach(var level in levels)
			{
				foreach(var logger in _loggers)
				{
					action(logger, level);
				}
			}
		}


		private void AddLogger(ActorRef logger, LogLevel logLevel, string logName)
		{
			//TODO: Send Initialize message to logger
			Log.GetSubscribeLevels(logLevel).ForEach(l => Subscribe(logger, l.Type));
			Publish(new DebugLogEvent(logName,GetType(),"Logger " + logName +" started"));
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
				StandardOutLogger.PrintError(new ErrorLogEvent(GetType().Name, GetType(), string.Format("Unknown StandardOutLogger LogLevel setting: {0}, defaulting to Error", logLevel), new LoggingException()), dateFormat);
				logLevel = LogLevel.Error;
			}
			Log.GetSubscribeLevels(logLevel).ForEach(level=>Subscribe(stdOutLogger,level.Type));
			_lock.Write(() =>
			{
				_loggers.Add(stdOutLogger);
				_logLevel = logLevel;
			});
		}

		//TODO: startDefaultLoggers
		//TODO: stopDefaultLoggers
	}

}