using System;
using System.Text.RegularExpressions;
using Aktris.Dispatching;
using Aktris.Exceptions;
using Aktris.Internals;
using Aktris.Internals.Logging;
using Aktris.Internals.Path;
using Aktris.JetBrainsAnnotations;
using Aktris.Settings;

namespace Aktris
{
	public abstract class ActorSystem : IActorCreator
	{
		private readonly string _name;
		private readonly IUniqueNameCreator _uniqueNameCreator;
		private readonly LocalActorRefFactory _localActorRefFactory;
		private readonly ActorRef _deadLetters;
		private readonly Func<IScheduler, Mailbox> _defaultMailboxCreator;
		private readonly Mailbox _deadLettersMailbox;
		private bool _isStarted;
		private GuardianActorRef _rootGuardian;
		private InternalActorRef _systemGuardian;
		private InternalActorRef _userGuardian;
		private readonly RootActorPath _rootPath;
		private readonly IScheduler _scheduler;
		private TempNodeHandler _tempNodeHandler;

		private readonly EventStream _eventStream;
		private ISettings _settings;
		private string _logSource;

		protected ActorSystem([NotNull] string name, [NotNull] IBootstrapper bootstrapper)
		{
			if(bootstrapper == null) throw new ArgumentNullException("bootstrapper");
			if(bootstrapper.UniqueNameCreator == null) throw new ArgumentException("IBootstrapper.UniqueNameCreator was null", "bootstrapper");
			if(bootstrapper.LocalActorRefFactory == null) throw new ArgumentException("IBootstrapper.LocalActorRefFactory was null", "bootstrapper");

			if(name == null) throw new ArgumentNullException("name");
			if(name.Length == 0) throw new ArgumentException("name");
			var nameLegal = new Regex("^[a-zA-Z0-9][a-zA-Z0-9-]*$");
			if(!nameLegal.IsMatch(name))
			{
				throw new ArgumentException(
					string.Format(
						"Invalid ActorSystem name [{0}], must contain only word characters (i.e. [a-zA-Z0-9] plus non-leading '-')", name),
					"name");
			}

			_name = name;
			_rootPath = new RootActorPath("/");
			_tempNodeHandler = new TempNodeHandler(_rootPath / "temp");
			_settings = bootstrapper.Settings;

			_uniqueNameCreator = bootstrapper.UniqueNameCreator;
			_localActorRefFactory = bootstrapper.LocalActorRefFactory;
			_deadLetters = bootstrapper.DeadLetterActorCreator(_rootPath / "_DeadLetter", this);
			_deadLettersMailbox = new DeadLetterMailbox(_deadLetters);
			_scheduler = bootstrapper.Scheduler;
			_defaultMailboxCreator = bootstrapper.DefaultMailboxCreator;
			_eventStream = new EventStream(_deadLetters, _settings.DebugEventStream);
			var standardOutLogger = new StandardOutLogger(new ChildActorPath(_rootPath, "_StandardOutLogger", LocalActorRef.UndefinedInstanceId), this);
			_eventStream.StartStandardOutLogger(standardOutLogger, _settings.StandardOutLoggerSettings);
			_logSource = "ActorSystem:" + name;
		}

		public string Name { get { return _name; } }
		//TODO: Maybe push these down to InternalActorSystem and make them public
		internal ActorRef DeadLetters { get { return _deadLetters; } }
		internal IUniqueNameCreator UniqueNameCreator { get { return _uniqueNameCreator; } }
		internal LocalActorRefFactory LocalActorRefFactory { get { return _localActorRefFactory; } }
		internal InternalActorRef RootGuardian { get { return _rootGuardian; } }
		internal InternalActorRef SystemGuardian { get { return _systemGuardian; } }
		internal InternalActorRef UserGuardian { get { return _userGuardian; } }
		internal Mailbox DeadLettersMailbox { get { return _deadLettersMailbox; } }
		internal IScheduler Scheduler { get { return _scheduler; } }

		protected TempNodeHandler TempNodeHandler { get { return _tempNodeHandler; } }


		public void Start()
		{
			_isStarted = true;
			_rootGuardian = CreateRootGuardian();
			_systemGuardian = CreateSystemGuardian(_rootGuardian);
			_userGuardian = CreateUserGuardian(_rootGuardian);
			_rootGuardian.Start();
			_eventStream.LogInfo(_logSource, this, "System started");
		}

		public ActorRef CreateActor(ActorCreationProperties actorCreationProperties, string name = null)
		{
			if(!_isStarted) throw new InvalidOperationException(string.Format("You must call {0}.Start() before creating an actor.", typeof(ActorSystem).Name));
			return _userGuardian.CreateActor(actorCreationProperties, name);
		}

		public Mailbox CreateDefaultMailbox()
		{
			return _defaultMailboxCreator(_scheduler);
		}

		private GuardianActorRef CreateRootGuardian()
		{
			var supervisor = new RootGuardianSupervisor(_rootPath, this);
			var rootGuardian = new GuardianActorRef(this, ActorCreationProperties.Create(() => new Guardian()), _rootPath, CreateDefaultMailbox(), supervisor);
			return rootGuardian;
		}

		private InternalActorRef CreateSystemGuardian(GuardianActorRef rootGuardian)
		{
			var systemGuardian = rootGuardian.CreateGuardian(() => new Guardian(), "system");
			return systemGuardian;
		}


		protected virtual InternalActorRef CreateUserGuardian(GuardianActorRef rootGuardian)
		{
			var userGuardian = rootGuardian.CreateGuardian(() => new Guardian(), "user");
			return userGuardian;
		}



	}
}