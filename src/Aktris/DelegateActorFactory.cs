using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris
{
	public class DelegateActorFactory : ActorFactory
	{
		private readonly Func<Actor> _factory;

		public DelegateActorFactory([NotNull] Func<Actor> factory)
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