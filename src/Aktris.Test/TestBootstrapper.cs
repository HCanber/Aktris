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
			DeadLetterActorCreator = path => new DeadLetterActorRef(path);
			DefaultMailboxCreator = () => new UnboundedMailbox(new SynchronousScheduler());
		}
		public IUniqueNameCreator UniqueNameCreator { get; set; }
		public LocalActorRefFactory LocalActorRefFactory { get; set; }
		public Func<ActorPath, ActorRef> DeadLetterActorCreator { get; set; }
		public Func<Mailbox> DefaultMailboxCreator { get; set; }
	}
}