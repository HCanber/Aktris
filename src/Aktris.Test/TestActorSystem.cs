using System;
using Aktris.Internals;

namespace Aktris.Test
{
	public class TestActorSystem : ActorSystem
	{
		public TestActorSystem()
			: this(null)
		{
		}

		public TestActorSystem(IBootstrapper bootstrapper)
			: base("test", bootstrapper ?? new TestBootstrapper())
		{
		}

		public ActorRefWithActor<TestActor> CreateTestActor(string name=null)
		{
			return CreateActor<TestActor>(() => new TestActor(), name);
		}

		public ActorRefWithActor<TActor> CreateActor<TActor>(Func<TActor> createActor, string name = null) where TActor : Actor
		{
			TActor actor = null;
			var actorRef = CreateActor(ActorCreationProperties.Create(() => { actor = createActor(); return actor; }), name);
			return new ActorRefWithActor<TActor>(actorRef, actor);
		}

		public ActorRefWithActor<TActor> CreateActor<TActor>(string name = null) where TActor : Actor, new()
		{
			TActor actor = null;
			var actorRef = CreateActor(ActorCreationProperties.Create(() => { actor = new TActor(); return actor; }), name);
			return new ActorRefWithActor<TActor>(actorRef, actor);
		}
	}
}