using System;
using Aktris.Internals.Path;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals.Logging
{
	public class StandardOutLogger : MinimalActorRef
	{
		private readonly ActorPath _path;
		private readonly ActorSystem _system;
		private readonly string _dateFormat;
		public const string DefaultDateFormat = StandardOutLoggerHelper.DefaultDateFormat;



		public StandardOutLogger(ActorPath path, ActorSystem system, string dateFormat = DefaultDateFormat)
		{
			_path = path;
			_system = system;
			_dateFormat = dateFormat ?? DefaultDateFormat;
		}

		public override ActorSystem System { get { return _system; } }

		public override string Name { get { return _path.Name; } }

		public override ActorPath Path { get { return _path; } }

		public override uint InstanceId { get { return _path.InstanceId; } }

		public override void Send([NotNull] object message, ActorRef sender)
		{
			Print(message);
		}

		private void Print(object message)
		{
			if(message == null) throw new ArgumentNullException("message");
			StandardOutLoggerHelper.Print(message, _dateFormat);
		}


		public override string ToString()
		{
			return "StandardOutLogger";
		}
	}
}