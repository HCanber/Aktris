namespace Aktris.Internals.SystemMessages
{
	public class TerminateActor : SystemMessage
	{
		public static readonly TerminateActor Instance = new TerminateActor();
		private TerminateActor() { }
	}
}