using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Aktris.Internals;
using Aktris.Internals.Children;
using Aktris.Supervision;
using Aktris.Test.TestHelpers;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Aktris.Test.Supervision
{
	// ReSharper disable once InconsistentNaming
	public class AllForOneSupervisorStrategy_Tests
	{
		[Fact]
		public void When_decider_says_Escalate_Then_no_calls_are_made_to_actor_and_false_is_returned()
		{
			var strategy = new AllForOneSupervisorStrategy(exception => SupervisorAction.Escalate);
			var actor = A.Fake<InternalActorRef>();
			var sibling = A.Fake<InternalActorRef>();
			var actorRestartInfo = new ChildRestartInfo(actor);
			var actorAndSiblings = new List<ChildRestartInfo> { new ChildRestartInfo(sibling), actorRestartInfo };
			var failureHandled = strategy.HandleFailure(actor, new Exception(), actorRestartInfo, actorAndSiblings);
			failureHandled.Should().BeFalse();
			A.CallTo(actor).MustNotHaveHappened();
			A.CallTo(sibling).MustNotHaveHappened();
		}

		[Fact]
		public void When_decider_says_Stop_Then_all_the_siblings_are_Stopped()
		{
			var strategy = new AllForOneSupervisorStrategy(exception => SupervisorAction.Stop);
			var actor = A.Fake<InternalActorRef>();
			var sibling = A.Fake<InternalActorRef>();
			var actorRestartInfo = new ChildRestartInfo(actor);
			var actorAndSiblings = new List<ChildRestartInfo> { new ChildRestartInfo(sibling), actorRestartInfo };
			var failureHandled = strategy.HandleFailure(actor, new Exception(), actorRestartInfo, actorAndSiblings);
			failureHandled.Should().BeTrue();
			A.CallTo(() => actor.Stop()).MustHaveHappened(Repeated.Exactly.Once);
			A.CallTo(() => sibling.Stop()).MustHaveHappened(Repeated.Exactly.Once);
		}

		[Fact]
		public void When_decider_says_Restart_Then_the_actor_is_Restarted_without_suspending_and_the_siblings_are_suspended_and_restarted()
		{
			var strategy = new AllForOneSupervisorStrategy(exception => SupervisorAction.Restart);
			var actor = A.Fake<InternalActorRef>();
			var sibling1 = A.Fake<InternalActorRef>();
			var sibling2 = A.Fake<InternalActorRef>();
			var actorRestartInfo = new ChildRestartInfo(actor);
			var actorAndSiblings = new List<ChildRestartInfo> { new ChildRestartInfo(sibling1), actorRestartInfo, new ChildRestartInfo(sibling2)};
			var cause = new Exception();
			var failureHandled = strategy.HandleFailure(actor, cause, actorRestartInfo, actorAndSiblings);
			failureHandled.Should().BeTrue();
			A.CallTo(() => actor.Restart(cause)).MustHaveHappened(Repeated.Exactly.Once);
			A.CallTo(() => actor.Suspend()).MustNotHaveHappened();
			A.CallTo(() => sibling1.Suspend()).MustHaveHappened(Repeated.Exactly.Once);
			A.CallTo(() => sibling1.Restart(cause)).MustHaveHappened(Repeated.Exactly.Once);
			A.CallTo(() => sibling2.Suspend()).MustHaveHappened(Repeated.Exactly.Once);
			A.CallTo(() => sibling2.Restart(cause)).MustHaveHappened(Repeated.Exactly.Once);
		}

		[Fact]
		public void When_decider_says_Resume_Then_the_actor_is_Resumed_but_not_the_siblings()
		{
			var strategy = new AllForOneSupervisorStrategy(exception => SupervisorAction.Resume);
			var actor = A.Fake<InternalActorRef>();
			var sibling = A.Fake<InternalActorRef>();
			var actorRestartInfo = new ChildRestartInfo(actor);
			var actorAndSiblings = new List<ChildRestartInfo> { new ChildRestartInfo(sibling), actorRestartInfo };
			var cause = new Exception();
			var failureHandled = strategy.HandleFailure(actor, cause, actorRestartInfo, actorAndSiblings);
			failureHandled.Should().BeTrue();
			A.CallTo(() => actor.Resume(cause)).MustHaveHappened(Repeated.Exactly.Once);
			A.CallTo(() => sibling.Resume(cause)).MustNotHaveHappened();
		}

	}
}