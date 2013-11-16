using Aktris.Dispatching;
using Aktris.Internals;
using FakeItEasy;
using Xunit;

namespace Aktris.Test.Internals
{
// ReSharper disable InconsistentNaming
	public class LocalActorRef_Tests
	{
		[Fact]
		public void When_calling_Start_Then_the_actor_is_attached_to_the_mailbox()
		{
			var mailbox = A.Fake<Mailbox>();
			var actorRef = new LocalActorRef(A.Fake<ActorCreationProperties>(), "test", mailbox);

			actorRef.Start();

			A.CallTo(() => mailbox.Attach(actorRef)).MustHaveHappened();
		}
	}
	// ReSharper restore InconsistentNaming
}