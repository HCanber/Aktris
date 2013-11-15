using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris
{
	public class DelegateActorCreationProperties : ActorCreationProperties
	{
		private readonly Func<Actor> _factory;

		public DelegateActorCreationProperties([NotNull] Func<Actor> factory)
		{
			if(factory == null) throw new ArgumentNullException("factory");
			_factory = factory;
		}

		public override Actor CreateNewActor()
		{
			return _factory();
		}
	}
}