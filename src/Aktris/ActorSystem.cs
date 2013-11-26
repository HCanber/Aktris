using System;
using System.Text.RegularExpressions;
using Aktris.Dispatching;
using Aktris.Exceptions;
using Aktris.Internals;
using Aktris.JetBrainsAnnotations;

namespace Aktris
{
	public abstract class ActorSystem : IActorCreator
	{
		private readonly string _name;
		private readonly IUniqueNameCreator _uniqueNameCreator;
		private readonly LocalActorRefFactory _localActorRefFactory;
		private readonly ActorRef _deadLetters;
		private readonly Func<Mailbox> _defaultMailboxCreator;
		private bool _isStarted;

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
			_uniqueNameCreator = bootstrapper.UniqueNameCreator;
			_localActorRefFactory = bootstrapper.LocalActorRefFactory;
			_deadLetters = bootstrapper.DeadLetterActorCreator();
			_defaultMailboxCreator = bootstrapper.DefaultMailboxCreator;
		}

		public string Name { get { return _name; } }
		public ActorRef DeadLetters { get { return _deadLetters; } }
		public IUniqueNameCreator UniqueNameCreator { get { return _uniqueNameCreator; } }
		public LocalActorRefFactory LocalActorRefFactory { get { return _localActorRefFactory; } }

		public void Start()
		{
			_isStarted = true;
		}

		public ActorRef CreateActor(ActorCreationProperties actorCreationProperties, string name=null)
		{
			if(!_isStarted) throw new InvalidOperationException(string.Format("You must call {0}.Start() before creating an actor.", typeof(ActorSystem).Name));

			if(name != null)
			{
				ActorNameValidator.EnsureNameIsValid(name);
			}
			else name = _uniqueNameCreator.GetNextRandomName();
			var actorRef = _localActorRefFactory.CreateActor(this, actorCreationProperties, name);
			actorRef.Start();
			return actorRef;
		}

		/// <summary>
		/// Creates a new <see cref="ActorSystem"/> with an optional name.
		/// </summary>
		/// <param name="name">[Optional] The name of the system. If left out or if <c>null</c> then the system will get the name "default".</param>
		/// <returns>The new system.</returns>
		public static ActorSystem Create(string name = null)
		{
			var systemFactory = DefaultActorSystemFactory.Instance;
			var system = systemFactory.Create(name ?? "default");
			return system;
		}

		public Mailbox CreateDefaultMailbox()
		{
			return _defaultMailboxCreator();
		}
	}
}