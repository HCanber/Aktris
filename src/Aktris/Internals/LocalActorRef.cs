using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals
{
	public class LocalActorRef : ActorRef
	{
		private readonly ActorFactory _actorFactory;
		private readonly string _name;

		public LocalActorRef([NotNull] ActorFactory actorFactory, [NotNull] string name)
		{
			if(actorFactory == null) throw new ArgumentNullException("actorFactory");
			if(name == null) throw new ArgumentNullException("name");
			_actorFactory = actorFactory;
			_name = name;
		}

		public string Name { get { return _name; } }
	}
}