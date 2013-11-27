using System;
using System.Collections.Generic;

namespace Aktris.Test
{
	public class TestActor : Actor
	{
		public List<Tuple<ActorRef, object>> ReceivedMessages=new List<Tuple<ActorRef, object>>();

		protected internal override bool HandleMessage(object message)
		{
			ReceivedMessages.Add(Tuple.Create(Sender.Unwrap(),message));
			return base.HandleMessage(message);
		}
	}
}