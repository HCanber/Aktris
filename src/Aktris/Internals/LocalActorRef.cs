using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals
{
	public class LocalActorRef : ILocalActorRef
	{
		private readonly ActorCreationProperties _actorCreationProperties;
		private readonly string _name;

		public LocalActorRef([NotNull] ActorCreationProperties actorCreationProperties, [NotNull] string name)
		{
			if(actorCreationProperties == null) throw new ArgumentNullException("actorCreationProperties");
			if(name == null) throw new ArgumentNullException("name");
			_actorCreationProperties = actorCreationProperties;
			_name = name;
		}

		public string Name { get { return _name; } }

		public void Start()
		{
		}
	}
}