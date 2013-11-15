using System;
using System.Text.RegularExpressions;
using Aktris.Internals;
using Aktris.JetBrainsAnnotations;

namespace Aktris
{
	public abstract class ActorSystem : IActorCreator
	{
		private readonly string _name;
		private readonly IUniqueNameCreator _uniqueNameCreator;

		protected ActorSystem([NotNull] string name, IUniqueNameCreator uniqueNameCreator)
		{
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
			_uniqueNameCreator = uniqueNameCreator;
		}

		public string Name { get { return _name; } }


		public ActorRef CreateActor(ActorFactory actorFactory, string name=null)
		{
			name = name ?? _uniqueNameCreator.GetNextRandomName();
			return new LocalActorRef(actorFactory,name);
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