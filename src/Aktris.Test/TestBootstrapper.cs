using System;
using Aktris.Dispatching;
using Aktris.Internals;

namespace Aktris.Test
{
	public class TestBootstrapper : IBootstrapper
	{
		public TestBootstrapper()
		{
			UniqueNameCreator = new UniqueNameCreator();
			LocalActorRefFactory = new DefaultLocalActorRefFactory();
			DeadLetterActorCreator = (path, system) => new DeadLetterActorRef(path, system);
			Scheduler = new SynchronousScheduler();
			DefaultMailboxCreator = scheduler => new UnboundedMailbox(scheduler);
		}
		public IUniqueNameCreator UniqueNameCreator { get; set; }
		public LocalActorRefFactory LocalActorRefFactory { get; set; }
		public Func<ActorPath, ActorSystem, ActorRef> DeadLetterActorCreator { get; set; }
		public Func<IScheduler, Mailbox> DefaultMailboxCreator { get; set; }
		public IScheduler Scheduler { get; set; }
	}
}