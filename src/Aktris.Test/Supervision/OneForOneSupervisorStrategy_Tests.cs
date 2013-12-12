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
	public class OneForOneSupervisorStrategy_Tests
	{
		[Fact]
		public void When_decider_says_Escalate_Then_no_calls_are_made_to_actor_and_false_is_returned()
		{
			var strategy = new OneForOneSupervisorStrategy(exception => SupervisorAction.Escalate);
			var actor = A.Fake<InternalActorRef>();
			var sibling = A.Fake<InternalActorRef>();
			var actorRestartInfo = new ChildRestartInfo(actor);
			var actorAndSiblings = new List<ChildRestartInfo> { new ChildRestartInfo(sibling), actorRestartInfo };
			var failureHandled = strategy.HandleFailure(actorRestartInfo, new Exception(), actorAndSiblings);
			failureHandled.Should().BeFalse();
			A.CallTo(actor).MustNotHaveHappened();
			A.CallTo(sibling).MustNotHaveHappened();
		}

		[Fact]
		public void When_decider_says_Stop_Then_the_actor_is_Stopped_but_not_the_siblings()
		{
			var strategy = new OneForOneSupervisorStrategy(exception => SupervisorAction.Stop);
			var actor = A.Fake<InternalActorRef>();
			var sibling = A.Fake<InternalActorRef>();
			var actorRestartInfo = new ChildRestartInfo(actor);
			var actorAndSiblings = new List<ChildRestartInfo> { new ChildRestartInfo(sibling), actorRestartInfo };
			var failureHandled = strategy.HandleFailure(actorRestartInfo, new Exception(), actorAndSiblings);
			failureHandled.Should().BeTrue();
			A.CallTo(() => actor.Stop()).MustHaveHappened(Repeated.Exactly.Once);
			A.CallTo(() => sibling.Stop()).MustNotHaveHappened();
		}

		[Fact]
		public void When_decider_says_Restart_Then_the_actor_is_Restarted_without_suspending_first_but_not_the_siblings()
		{
			var strategy = new OneForOneSupervisorStrategy(exception => SupervisorAction.Restart);
			var actor = A.Fake<InternalActorRef>();
			var sibling = A.Fake<InternalActorRef>();
			var actorRestartInfo = new ChildRestartInfo(actor);
			var actorAndSiblings = new List<ChildRestartInfo> { new ChildRestartInfo(sibling), actorRestartInfo };
			var cause = new Exception();
			var failureHandled = strategy.HandleFailure(actorRestartInfo, cause, actorAndSiblings);
			failureHandled.Should().BeTrue();
			A.CallTo(() => actor.Restart(cause)).MustHaveHappened(Repeated.Exactly.Once);
			A.CallTo(() => actor.Suspend()).MustNotHaveHappened();
			A.CallTo(() => sibling.Restart(cause)).MustNotHaveHappened();
			A.CallTo(() => sibling.Suspend()).MustNotHaveHappened();
		}

		[Fact]
		public void When_decider_says_Resume_Then_the_actor_is_Resumed_but_not_the_siblings()
		{
			var strategy = new OneForOneSupervisorStrategy(exception => SupervisorAction.Resume);
			var actor = A.Fake<InternalActorRef>();
			var sibling = A.Fake<InternalActorRef>();
			var actorRestartInfo = new ChildRestartInfo(actor);
			var actorAndSiblings = new List<ChildRestartInfo> { new ChildRestartInfo(sibling), actorRestartInfo };
			var cause = new Exception();
			var failureHandled = strategy.HandleFailure(actorRestartInfo, cause, actorAndSiblings);
			failureHandled.Should().BeTrue();
			A.CallTo(() => actor.Resume(cause)).MustHaveHappened(Repeated.Exactly.Once);
			A.CallTo(() => sibling.Resume(cause)).MustNotHaveHappened();
		}

	}
}