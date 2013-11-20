using System;
using System.Collections.Immutable;

namespace Aktris.Internals
{
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