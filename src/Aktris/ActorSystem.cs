using System;
using System.Text.RegularExpressions;
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
		private const string _NameExtraCharacter = @"-_=+,.!~";
		private static readonly Regex _ValidNameRegex = new Regex(@"^[[:alnum:]]([[:alnum:]" + _NameExtraCharacter + @"])*", RegexOptions.Compiled);

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
		}

		public string Name { get { return _name; } }


		public ActorRef CreateActor(ActorFactory actorFactory, string name=null)
		{
			if(name != null)
			{
				EnsureNameIsValid(name);
			}
			else name = _uniqueNameCreator.GetNextRandomName();
			var actorRef = _localActorRefFactory.CreateActor(actorFactory, name);
			actorRef.Start();
			return actorRef;
		}

		private void EnsureNameIsValid(string name)
		{
			if(string.IsNullOrEmpty(name)) throw new InvalidActorNameException("The name may not be empty string.");
			if(!_ValidNameRegex.IsMatch(name)) throw new InvalidActorNameException(string.Format("Invalid name \"{1}\". The name must start with alpha-numerical (a-zAZ-09) then followed by alphanumerical including the characters {0}", _NameExtraCharacter, name));
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
	}
}