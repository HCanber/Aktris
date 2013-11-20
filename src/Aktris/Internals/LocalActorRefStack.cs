using System;
using System.Collections.Immutable;

namespace Aktris.Internals
{
	/// <summary>
	/// This class is used during creation of actors. Before the actor's 
	/// constructor is called, the LocalActorRef instance is pushed onto the 
	/// stack. Then the actor's constructor is called, and in which it pops the 
	/// LocalActorRef off the stack and saves it. 
	/// If no LocalActorRef exists on the stack it means were calling the actor's
	/// constructor directly, which is not allowed, and an exception is thrown.
	/// 
	/// The stack is per thread so no concurrency or interference should occur.
	/// </summary>
	internal static class LocalActorRefStack
	{
		[ThreadStatic]
		private static ImmutableStack<LocalActorRef> _actorStackDoNotCallMeDirectly;


		internal static void PushActorRefToStack(LocalActorRef actorRef)
		{
			InterlockedSpin.Swap(ref _actorStackDoNotCallMeDirectly, st =>
				st == null
					? ImmutableStack.Create(actorRef)
					: st.Push(actorRef));
		}

		internal static void PopActorAndMarkerFromStack()
		{
			InterlockedSpin.Swap(ref _actorStackDoNotCallMeDirectly, st =>
				st == null
					? null
					: st.Peek() == null		// if first item is null, i.e. a marker
						? st.Pop().Pop()  // then pop that value, 
						: st.Pop());      // otherwise pop only the actor
		}

		internal static void MarkActorRefConsumedInStack()
		{
			PushActorRefToStack(null);
		}

		internal static ImmutableStack<LocalActorRef> GetActorRefStackForTestingOnly()
		{
			return _actorStackDoNotCallMeDirectly;
		}

		internal static bool TryGetActorRefFromStack(out LocalActorRef actorRef)
		{
			var stack = _actorStackDoNotCallMeDirectly;
			if(stack == null || stack.IsEmpty)
			{
				actorRef = null;
				return false;
			}
			actorRef = stack.Peek();
			return true;
		}
	}
}