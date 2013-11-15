using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals
{
	public class InternalActorSystem : ActorSystem
	{
		public InternalActorSystem([NotNull] string name, IBootstrapper bootstrapper) : base(name, bootstrapper)
		{
		}
	}
}