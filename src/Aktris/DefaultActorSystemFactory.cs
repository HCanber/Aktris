using System;
using Aktris.Dispatching;
using Aktris.Internals;

namespace Aktris
{
	public class DefaultActorSystemFactory : ActorSystemFactory, IBootstrapper
	{
		private static readonly DefaultActorSystemFactory _Instance = new DefaultActorSystemFactory();

		/// <summary>This constructor should be hidden from the outside</summary>
		private DefaultActorSystemFactory()
		{
			UniqueNameCreator=new UniqueNameCreator();
			LocalActorRefFactory = new DefaultLocalActorRefFactory();
			DeadLetterActorCreator = (path,system) => new DeadLetterActorRef(path,system);
			Scheduler = new ThreadPoolScheduler();
			DefaultMailboxCreator = scheduler => new UnboundedMailbox(scheduler);
		}

		public static DefaultActorSystemFactory Instance { get { return _Instance; } }


		public ActorSystem Create(string name)
		{
			return new InternalActorSystem(name, this);
		}

		public IUniqueNameCreator UniqueNameCreator { get; set; }
		public LocalActorRefFactory LocalActorRefFactory { get; set; }
		public Func<ActorPath, ActorSystem, ActorRef> DeadLetterActorCreator { get; set; }
		public Func<IScheduler,Mailbox> DefaultMailboxCreator { get; set; }
		public IScheduler Scheduler { get; set; }
	}
}