using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals
{
	public class InternalActorSystem : ActorSystem
	{
		public InternalActorSystem([NotNull] string name) : this(name, new UniqueNameCreator())
		{
		}

		public InternalActorSystem([NotNull] string name, IUniqueNameCreator uniqueNameCreator) : base(name, uniqueNameCreator)
		{
		}
	}
}