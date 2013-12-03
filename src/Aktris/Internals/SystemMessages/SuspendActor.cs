namespace Aktris.Internals.SystemMessages
{
	public class SuspendActor : SystemMessage
	{
		public static readonly SuspendActor Instance = new SuspendActor();
		private SuspendActor() { }
	}
}