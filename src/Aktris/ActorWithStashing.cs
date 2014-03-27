using System;

namespace Aktris
{
	public class ActorWithStashing : Actor
	{
		private static readonly uint? UnlimitedCapacity = null;
		private Stashing _stashing;

		public ActorWithStashing()
			: this(UnlimitedCapacity)
		{
		}

		public ActorWithStashing(uint? stashingCapacity)
		{
			_stashing = new Stashing(InternalSelf, stashingCapacity);
		}

		/// <summary>
		/// Stashes the current message (the last message received)
		/// </summary>
		protected void Stash()
		{
			_stashing.Stash(InternalSelf.CurrentMessage);
		}

		/// <summary>
		/// Prepends all stashed messages to the mailbox and then clears the stash. This means they end up first in the mailbox and will be processed as the next messages.
		/// </summary>
		protected void UnstashAll()
		{
			_stashing.UnstashAll();
		}

		/// <summary>
		///  Prepends all messages in the stash to the mailbox, clears the stash and stops all children.
		/// </summary>
		protected internal override void PreRestart(Exception cause, object message, ActorRef optionalSender)
		{
			try
			{
				UnstashAll();
			}
			finally
			{
				base.PreRestart(cause, message,optionalSender);
			}
		}
	}
}