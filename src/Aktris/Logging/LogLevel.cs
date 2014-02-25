using System;

namespace Aktris.Logging
{
	[Flags]
	public enum LogLevel
	{
		Off     = 1 << 0,
		Error   = 1 << 1,
		Warning = 1 << 2,
		Info    = 1 << 3,
		Debug   = 1 << 4,

		WarningAndMoreSevere = Warning | Error,
		InfoAndMoreSevere = Info | WarningAndMoreSevere,
		DebugAndMoreSevere = Debug | InfoAndMoreSevere,
		All = DebugAndMoreSevere,
	}
}