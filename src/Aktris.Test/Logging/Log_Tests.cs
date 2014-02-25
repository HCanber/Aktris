using System;
using Aktris.Logging;
using FluentAssertions;
using Xunit;

namespace Aktris.Test.Logging
{
	public class Log_Tests
	{
		[Fact]
		public void Test_AllLevels()
		{
			Log.AllLevels.Should().HaveCount(4)
				.And.Contain(LogLevel.Error)
				.And.Contain(LogLevel.Warning)
				.And.Contain(LogLevel.Info)
				.And.Contain(LogLevel.Debug);
		}

		[Fact]
		public void Test_AllLevelsWithType()
		{
			Log.AllLevelsWithType.Should().HaveCount(4)
				.And.Contain(LogLevelWithType.Error)
				.And.Contain(LogLevelWithType.Warning)
				.And.Contain(LogLevelWithType.Info)
				.And.Contain(LogLevelWithType.Debug);
		}

		[Fact]
		public void Test_GetSubscribeLevels_Off()
		{
			Log.GetSubscribeLevels(LogLevel.Off).Should().BeEmpty();
		}

		[Fact]
		public void Test_GetSubscribeLevels_Error()
		{
			Log.GetSubscribeLevels(LogLevel.Error).Should().HaveCount(1)
				.And.Contain(LogLevelWithType.Error);
		}

		[Fact]
		public void Test_GetSubscribeLevels_Warning()
		{
			Log.GetSubscribeLevels(LogLevel.Warning).Should().HaveCount(2)
				.And.Contain(LogLevelWithType.Warning)
				.And.Contain(LogLevelWithType.Error);
		}

		[Fact]
		public void Test_GetSubscribeLevels_Info()
		{
			Log.GetSubscribeLevels(LogLevel.Info).Should().HaveCount(3)
				.And.Contain(LogLevelWithType.Info)
				.And.Contain(LogLevelWithType.Warning)
				.And.Contain(LogLevelWithType.Error);
		}

		[Fact]
		public void Test_GetSubscribeLevels_Debug()
		{
			Log.GetSubscribeLevels(LogLevel.Debug).Should().HaveCount(4)
				.And.Contain(LogLevelWithType.Debug)
				.And.Contain(LogLevelWithType.Info)
				.And.Contain(LogLevelWithType.Warning)
				.And.Contain(LogLevelWithType.Error);
		}
	}
}