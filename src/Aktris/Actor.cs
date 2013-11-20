using System;
using Aktris.Exceptions;
using Aktris.Internals;

namespace Aktris
{
// ReSharper disable once InconsistentNaming
	public abstract class Actor
	{
		protected Actor()
		{
			LocalActorRef actorRef;
			if(!LocalActorRefStack.TryGetActorRefFromStack(out actorRef))
			{
				throw new InvalidOperationException(StringFormat.SafeFormat("Cannot create a new instance of type {0} directly using new(). An actor can only be created via the CreateActor methods.", GetType().FullName));
			}
			LocalActorRefStack.MarkActorRefConsumedInStack();
		}

		protected internal SenderActorRef Sender { get; internal set; }

		internal protected virtual void Receive(object message)
		{
			throw new System.NotImplementedException();
		// ReSharper disable once VirtualMemberNeverOverriden.Global   Init is virtual in order to be mockable
		internal virtual void Init()
		{
		}
		}
	}
}