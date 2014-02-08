using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Aktris.Internals.Helpers;
using Aktris.Internals.Logging;

namespace Aktris.Logging
{
	public static class Log
	{
		private static readonly IReadOnlyCollection<LogLevel> _AllLevels;
		private static readonly IReadOnlyCollection<LogLevelWithType> _AllLevelsWithType;
		private static readonly IReadOnlyCollection<LogLevelWithType>[] _SubscribeLevels;

		static Log()
		{
			_AllLevels = (new[] { LogLevel.Error, LogLevel.Warning, LogLevel.Info, LogLevel.Debug }).ToReadOnlyCollection();
			var allLevelsWithType =(new[] { LogLevelWithType.Error, LogLevelWithType.Warning, LogLevelWithType.Info, LogLevelWithType.Debug }).ToReadOnlyCollection();
			var subscribeLevels=new IReadOnlyCollection<LogLevelWithType>[5];
			subscribeLevels[0] = EmptyReadonlyCollection<LogLevelWithType>.Instance;
			subscribeLevels[1] = allLevelsWithType.Where(l => l <= 1).ToReadOnlyCollection();
			subscribeLevels[2] = allLevelsWithType.Where(l => l <= 2).ToReadOnlyCollection();
			subscribeLevels[3] = allLevelsWithType.Where(l => l <= 3).ToReadOnlyCollection();
			subscribeLevels[4] = allLevelsWithType.Where(l => l <= 4).ToReadOnlyCollection();
			_SubscribeLevels = subscribeLevels;
			_AllLevelsWithType = allLevelsWithType;
		}

		public static IReadOnlyCollection<LogLevel> AllLevels { get { return _AllLevels; } }
		public static IReadOnlyCollection<LogLevelWithType> AllLevelsWithType { get { return _AllLevelsWithType; } }

		public static IReadOnlyCollection<LogLevelWithType> GetSubscribeLevels(LogLevel logLevel)
		{
			return _SubscribeLevels[(int) logLevel];
		}

		public static bool IsValidLogLevel(LogLevel logLevel)
		{
			return logLevel >= LogLevel.Off && logLevel <= LogLevel.Debug;
		}
	}

	public sealed class LogLevelWithType : IComparable<int>
	{
		private readonly LogLevel _logLevel;
		private readonly Type _type;
		private static readonly LogLevelWithType _Error=new LogLevelWithType(LogLevel.Error, typeof(ErrorLogEvent));
		private static readonly LogLevelWithType _Warning=new LogLevelWithType(LogLevel.Warning, typeof(WarningLogEvent));
		private static readonly LogLevelWithType _Info=new LogLevelWithType(LogLevel.Info, typeof(InfoLogEvent));
		private static readonly LogLevelWithType _Debug=new LogLevelWithType(LogLevel.Debug, typeof(DebugLogEvent));

		private LogLevelWithType(LogLevel logLevel, Type type)
		{
			_logLevel = logLevel;
			_type = type;
		}

		public LogLevel LogLevel { get { return _logLevel; } }
		public Type Type { get { return _type; } }

		public static LogLevelWithType Error { get { return _Error; } }
		public static LogLevelWithType Warning { get { return _Warning; } }
		public static LogLevelWithType Info { get { return _Info; } }
		public static LogLevelWithType Debug { get { return _Debug; } }

		public static implicit operator LogLevel(LogLevelWithType level) { return level._logLevel; }
		public static implicit operator Type(LogLevelWithType level) { return level._type; }

		public static bool operator <(LogLevelWithType logLevel, int value)
		{
			return ((int)logLevel._logLevel).CompareTo(value) < 0;
		}
		public static bool operator <=(LogLevelWithType logLevel, int value)
		{
			return ((int)logLevel._logLevel).CompareTo(value) <= 0;
		}

		public static bool operator >(LogLevelWithType logLevel, int value)
		{
			return ((int)logLevel._logLevel).CompareTo(value) > 0;
		}
		public static bool operator >=(LogLevelWithType logLevel, int value)
		{
			return ((int)logLevel._logLevel).CompareTo(value) >= 0;
		}

		public static bool operator ==(LogLevelWithType logLevel, int value)
		{
			return logLevel!=null && ((int)logLevel._logLevel) == value;
		}

		public static bool operator !=(LogLevelWithType logLevel, int value)
		{
			return logLevel==null || ((int)logLevel._logLevel) != value;
		}

		public int CompareTo(int other)
		{
			return ((int) _logLevel).CompareTo(other);
		}
	}
}