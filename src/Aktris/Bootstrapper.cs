using System;
using Aktris.Dispatching;
using Aktris.Internals;
using Aktris.Settings;

namespace Aktris
{
	public class Bootstrapper : IBootstrapper
	{
		public const string DefaultSystemName = "default";
		private static readonly Bootstrapper _Instance = new Bootstrapper();

		public Bootstrapper()
		{
			UniqueNameCreator=new UniqueNameCreator();
			LocalActorRefFactory = new DefaultLocalActorRefFactory();
			DeadLetterActorCreator = (path,system) => new DeadLetterActorRef(path, system);
			ActionScheduler=new ThreadPoolActionScheduler();
			DefaultMailboxCreator = scheduler => new UnboundedMailbox(ActionScheduler);
			Settings=new Settings.Settings();
			Scheduler = new TaskBasedScheduler();
		}

		public static Bootstrapper Instance { get { return _Instance; } }


		public ActorSystem CreateSystem(string name=DefaultSystemName)
		{
			return CreateSystem(name ?? DefaultSystemName, this);
		}

		public ActorSystem CreateSystemWithSettings(Action<Settings.Settings> settingsUpdater, string name = DefaultSystemName)
		{
			var clone = DeepClone();
			settingsUpdater(clone.Settings);
			return CreateSystem(name, clone);
		}

		private static ActorSystem CreateSystem(string name, IBootstrapper bootstrapper)
		{
			return new InternalActorSystem(name, bootstrapper);
		}

		public IUniqueNameCreator UniqueNameCreator { get; set; }
		public LocalActorRefFactory LocalActorRefFactory { get; set; }
		public Func<ActorPath, ActorSystem, ActorRef> DeadLetterActorCreator { get; set; }
		public Func<IActionScheduler, Mailbox> DefaultMailboxCreator { get; set; }
		public IActionScheduler ActionScheduler { get; set; }
		public IScheduler Scheduler { get; set; }

		public Settings.Settings Settings { get; set; }

		ISettings IBootstrapper.Settings { get { return Settings; } }

		protected virtual Bootstrapper DeepClone()
		{
			var clone = (Bootstrapper)MemberwiseClone();
			clone.Settings = Settings.DeepClone();
			return clone;
		}
	}
}