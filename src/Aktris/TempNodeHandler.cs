using System.Threading;
using Aktris.Internals;

namespace Aktris
{
	public class TempNodeHandler
	{
		private readonly ActorPath _path;
		private long _tempNodeNumber;

		public TempNodeHandler(ActorPath path)
		{
			_path = path;
		}

		public ActorPath CreatedNewTempPath()
		{
			return _path / CreateTempName();
		}

		protected string CreateTempName()
		{
			var tempNumber = Interlocked.Increment(ref _tempNodeNumber);
			var tempName = Base64Helper.Encode(tempNumber);
			return tempName;
		}
	}
}