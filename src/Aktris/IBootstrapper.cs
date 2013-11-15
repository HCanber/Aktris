using Aktris.Internals;

namespace Aktris
{
	public interface IBootstrapper
	{
		IUniqueNameCreator UniqueNameCreator { get; }
		DefaultLocalActorRefFactory LocalActorRefFactory { get; }
	}
}