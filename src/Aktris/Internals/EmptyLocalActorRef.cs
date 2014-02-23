using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals
{
	public abstract class EmptyLocalActorRef : MinimalActorRef
	{
		private readonly ActorPath _path;
		private readonly ActorSystem _actorSystem;

		public EmptyLocalActorRef([NotNull] ActorPath path, ActorSystem actorSystem)
		{
			if(path == null) throw new ArgumentNullException("path");
			_path = path;
			_actorSystem = actorSystem;
		}

		public override string Name { get { return _path.Name; } }

		public override ActorPath Path { get { return _path; } }

		public override uint InstanceId { get { return _path.InstanceId; } }

		public override ActorSystem System { get { return _actorSystem; } }
	}
}