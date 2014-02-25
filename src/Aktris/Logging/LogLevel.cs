using System;

namespace Aktris.Logging
{
	[Flags]
	public enum LogLevel
	{
		Off     = 0,
		Error   = 1 << 0,
		Warning = 1 << 1,
		Info    = 1 << 2,
		Debug   = 1 << 3,

		WarningAndMoreSevere = Warning | Error,
		InfoAndMoreSevere = Info | WarningAndMoreSevere,
		DebugAndMoreSevere = Debug | InfoAndMoreSevere,
		All = DebugAndMoreSevere,
	}
}