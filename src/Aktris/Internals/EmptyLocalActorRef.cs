using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals
{
	public abstract class EmptyLocalActorRef : MinimalActorRef
	{
		private readonly ActorPath _path;

		public EmptyLocalActorRef([NotNull] ActorPath path)
		{
			if(path == null) throw new ArgumentNullException("path");
			_path = path;
		}

		public override string Name { get { return _path.Name; } }

		public override ActorPath Path { get { return _path; } }

		public override uint InstanceId { get { return _path.InstanceId; } }
	}
}