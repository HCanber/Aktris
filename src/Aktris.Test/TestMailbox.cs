using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Aktris.Dispatching;
using Aktris.Internals;
using Aktris.Internals.SystemMessages;

namespace Aktris.Test
{
	public class TestMailbox : Mailbox, IEquatable<Mailbox>
	{
		private readonly Mailbox _mailbox;
		private readonly object _stateLock = new object();
		public List<Tuple<StateChange, State>> States { get; private set; }

		public TestMailbox(Mailbox mailbox)
		{
			_mailbox = mailbox;
			States = new List<Tuple<StateChange, State>>() { Tuple.Create(StateChange.Initial, new State()) };
		}

		public bool IsSuspended { get { return _mailbox.IsSuspended; } }

		public List<State> GetStateChangesFor(StateChange state)
		{
			return GetStateChangesFor(t => t.Item1 == state);
		}

		public List<T> GetEnquedSystemMessagesOfType<T>() where T : class, SystemMessage
		{
			return GetStateChangesFor(StateChange.EnqueueSystemMessage, state => { var m = state.GetLastEnqueuedSystemMessage(); return m != null && m.Message is T; }).Select(s=>(T)s.GetLastEnqueuedSystemMessage().Message).ToList();
		}

		public List<State> GetStateChangesForEnquingSystemMessagesOfType<T>()
		{
			return GetStateChangesFor(StateChange.EnqueueSystemMessage, state => { var m = state.GetLastEnqueuedSystemMessage(); return m != null && m.Message is T; });
		}

		public List<State> GetStateChangesFor(StateChange state, Predicate<State> isCorrectState)
		{
			return GetStateChangesFor(t => t.Item1 == state && isCorrectState(t.Item2));
		}

		public List<State> GetStateChangesFor(Predicate<Tuple<StateChange, State>> predicate)
		{
			return States.Where(t => predicate(t)).Select(t => t.Item2).ToList();
		}

		void Mailbox.SetActor(InternalActorRef actor)
		{
			ChangeState(StateChange.SetActor, s => s.SetActor(actor));
			_mailbox.SetActor(actor);
		}

		void Mailbox.Enqueue(Envelope envelope)
		{
			ChangeState(StateChange.Enqueue, s => s.EnqueueMessage(envelope));
			_mailbox.Enqueue(envelope);
		}

		void Mailbox.EnqueueSystemMessage(SystemMessageEnvelope envelope)
		{
			ChangeState(StateChange.EnqueueSystemMessage, s => s.EnqueueSystemMessage(envelope));
			_mailbox.EnqueueSystemMessage(envelope);
		}

		void Mailbox.Suspend(InternalActorRef actor)
		{
			ChangeState(StateChange.Suspend, s => s.IncreaseNumberOfSuspendCalls());
			_mailbox.Suspend(actor);
		}

		void Mailbox.Resume(InternalActorRef actor)
		{
			ChangeState(StateChange.Resume, s => s.IncreaseNumberOfResumeCalls());
			_mailbox.Resume(actor);
		}

		void Mailbox.DetachActor(InternalActorRef actor)
		{
			ChangeState(StateChange.DetachActor, s => s.Actor==actor ? s.SetActor(null) : s);
			_mailbox.DetachActor(actor);
		}

		public bool Equals(Mailbox other)
		{
			return _mailbox.Equals(other);
		}

		public override bool Equals(object obj)
		{
			var mailbox = obj as Mailbox;
			if(mailbox == null) return false;
			return Equals(mailbox);
		}

		private void ChangeState(StateChange stateChange, Func<State, State> stateChanger)
		{
			lock(_stateLock)
			{
				var lastIndex = States.Count - 1;
				var lastState = States[lastIndex].Item2;
				var newState = stateChanger(lastState);
				States.Add(Tuple.Create(stateChange, newState));
			}
		}

		public enum StateChange
		{
			Initial, SetActor, Enqueue, EnqueueSystemMessage, Suspend, Resume, DetachActor
		}

		public class State
		{
			private readonly InternalActorRef _actor;
			private readonly int _numberOfSuspendCalls;
			private readonly int _numberOfResumeCalls;
			private readonly ImmutableList<Envelope> _enquedMessages;
			private readonly ImmutableList<SystemMessageEnvelope> _enquedSystemMessages;

			public State()
			{
				_enquedMessages = ImmutableList<Envelope>.Empty;
				_enquedSystemMessages = ImmutableList<SystemMessageEnvelope>.Empty;
			}
			public State(InternalActorRef actor, int numberOfSuspendCalls, int numberOfResumeCalls, ImmutableList<Envelope> enquedMessages, ImmutableList<SystemMessageEnvelope> enquedSystemMessages)
			{
				_actor = actor;
				_numberOfSuspendCalls = numberOfSuspendCalls;
				_numberOfResumeCalls = numberOfResumeCalls;
				_enquedMessages = enquedMessages;
				_enquedSystemMessages = enquedSystemMessages;
			}

			public InternalActorRef Actor { get { return _actor; } }
			public int NumberOfSuspendCalls { get { return _numberOfSuspendCalls; } }
			public int NumberOfResumeCalls { get { return _numberOfResumeCalls; } }
			public ImmutableList<Envelope> EnquedMessages { get { return _enquedMessages; } }
			public ImmutableList<SystemMessageEnvelope> EnquedSystemMessages { get { return _enquedSystemMessages; } }

			public SystemMessageEnvelope GetLastEnqueuedSystemMessage()
			{
				var count = _enquedSystemMessages.Count;
				return count == 0 ? null : _enquedSystemMessages[count - 1];
			}

			public Envelope GetLastEnqueuedMessage()
			{
				var count = _enquedMessages.Count;
				return count == 0 ? null : _enquedMessages[count - 1];
			}

			public State SetActor(InternalActorRef actor)
			{
				return new State(actor, _numberOfSuspendCalls, _numberOfResumeCalls, _enquedMessages, _enquedSystemMessages);
			}

			public State IncreaseNumberOfSuspendCalls()
			{
				return new State(_actor, _numberOfSuspendCalls + 1, _numberOfResumeCalls, _enquedMessages, _enquedSystemMessages);
			}
			public State IncreaseNumberOfResumeCalls()
			{
				return new State(_actor, _numberOfSuspendCalls, _numberOfResumeCalls + 1, _enquedMessages, _enquedSystemMessages);
			}
			public State EnqueueMessage(Envelope message)
			{
				return new State(_actor, _numberOfSuspendCalls, _numberOfResumeCalls, _enquedMessages.Add(message), _enquedSystemMessages);
			}
			public State EnqueueSystemMessage(SystemMessageEnvelope message)
			{
				return new State(_actor, _numberOfSuspendCalls, _numberOfResumeCalls, _enquedMessages, _enquedSystemMessages.Add(message));
			}
		}
	}
}