namespace Aktris.Settings
{
	public class Settings : ISettings
	{
		public Settings()
		{
			StandardOutLoggerSettings = new StandardOutLoggerSettings();
		}
		public bool DebugEventStream { get; set; }
		public StandardOutLoggerSettings StandardOutLoggerSettings { get; set; }
		IStandardOutLoggerSettings ISettings.StandardOutLoggerSettings { get { return StandardOutLoggerSettings; } }

		public Settings DeepClone()
		{
			var clone = (Settings)MemberwiseClone();
			clone.StandardOutLoggerSettings = StandardOutLoggerSettings.DeepClone();
			return clone;
		}
	}
}