using System;
using System.Collections.Generic;
using System.Linq;
using Aktris.Internals.Children;
using Aktris.Internals.Helpers;

namespace Aktris.Supervision
{
	public class AllForOneSupervisorStrategy : DeciderSupervisorStrategy
	{
		private static readonly AllForOneSupervisorStrategy _DefaultInstance=new AllForOneSupervisorStrategy(SupervisorStrategy.DefaultDecider);

		public AllForOneSupervisorStrategy(Func<Exception, SupervisorAction> decider = null, uint? maxNrOfRetries = null)
			: base(decider, maxNrOfRetries)
		{
		}

		public static AllForOneSupervisorStrategy DefaultAllForOne { get { return _DefaultInstance; } }


		protected override void HandleRestart(ActorRef failingActor, Exception cause, ChildRestartInfo restartInfo, IReadOnlyCollection<ChildRestartInfo> actorWithSiblings)
		{
			if(actorWithSiblings.Count > 0)
			{
				var okToRestartAllChildren = actorWithSiblings.All(IsOkToRestart);
				if(okToRestartAllChildren)
				{
					actorWithSiblings.ForEach(c => RestartActor(c.Child, cause,shouldSuspendFirst: c.Child!=failingActor));
				}
				else
				{
					actorWithSiblings.ForEach(c => StopActor(c.Child, cause));
				}
			}
		}

		protected override void HandleStop(ActorRef failingActor, Exception cause, ChildRestartInfo restartInfo, IReadOnlyCollection<ChildRestartInfo> actorWithSiblings)
		{
			actorWithSiblings.ForEach(c => StopActor(c.Child, cause));
		}

	}
}