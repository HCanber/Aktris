using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Aktris.Exceptions;

namespace Aktris.Internals.Children
{
	public abstract class ChildrenCollection
	{
		public abstract ChildrenCollection ReserveName(string name);
		public abstract ChildrenCollection ReleaseName(string name);

		public abstract int Count { get; }
		public virtual bool IsEmpty { get { return Count == 0; } }

		public abstract bool TryGetByRef(ActorRef actorRef, out ChildRestartInfo info);
		public abstract bool TryGetByName(string actorName, out ChildInfo info);
		public abstract ChildrenCollection AddChild(ChildRestartInfo childRestartInfo);
		public abstract ChildrenCollection AddChild(string name, ChildRestartInfo childRestartInfo);
		public abstract ChildrenCollection RemoveChild(ActorRef child);
		public abstract IEnumerable<InternalActorRef> GetChildrenRefs();
		public abstract IEnumerable<ChildRestartInfo> GetChildren();
		public abstract ChildrenCollection IsAboutToTerminate(ActorRef child);
		public abstract bool HasChildrenThatAreTerminating();
		public abstract IEnumerable<KeyValuePair<string, ChildRestartInfo>> GetChildrenWithName();
	}

	public class EmptyChildrenCollection : ChildrenCollection
	{
		public static readonly EmptyChildrenCollection Instance = new EmptyChildrenCollection();
		protected EmptyChildrenCollection() { }
		public override int Count { get { return 0; } }

		public override ChildrenCollection ReserveName(string name)
		{
			return NormalChildrenCollection.CreateNew(ImmutableDictionary<string, ChildInfo>.Empty.Add(name, ChildNameReserved.Instance));
		}

		public override ChildrenCollection ReleaseName(string name)
		{
			return this;
		}

		public override bool TryGetByName(string actorName, out ChildInfo info)
		{
			info = null;
			return false;
		}

		public override bool TryGetByRef(ActorRef actorRef, out ChildRestartInfo info)
		{
			info = null;
			return false;
		}

		public override ChildrenCollection AddChild(ChildRestartInfo childRestartInfo)
		{
			return NormalChildrenCollection.CreateNew(ImmutableDictionary<string, ChildInfo>.Empty.Add(childRestartInfo.Child.Name, childRestartInfo));
		}

		public override ChildrenCollection AddChild(string name, ChildRestartInfo childRestartInfo)
		{
			return NormalChildrenCollection.CreateNew(ImmutableDictionary<string, ChildInfo>.Empty.Add(name, childRestartInfo));
		}

		public override ChildrenCollection RemoveChild(ActorRef child)
		{
			return this;
		}

		public override IEnumerable<InternalActorRef> GetChildrenRefs()
		{
			yield break;
		}

		public override IEnumerable<ChildRestartInfo> GetChildren()
		{
			yield break;
		}

		public override IEnumerable<KeyValuePair<string, ChildRestartInfo>> GetChildrenWithName()
		{
			yield break;
		}

		public override ChildrenCollection IsAboutToTerminate(ActorRef child)
		{
			return this;
		}

		public override bool HasChildrenThatAreTerminating()
		{
			return false;
		}
	}

	public class NormalChildrenCollection : ChildrenCollection
	{
		private readonly ImmutableDictionary<string, ChildInfo> _children;

		protected NormalChildrenCollection(ImmutableDictionary<string, ChildInfo> children)
		{
			_children = children;
		}

		public override int Count { get { return Children.Count; } }

		public ImmutableDictionary<string, ChildInfo> Children { get { return _children; } }

		public override ChildrenCollection ReserveName(string name)
		{
			if(Children.ContainsKey(name))
				throw new InvalidActorNameException(string.Format("Actor name \"{0}\" is not unique!", name));

			return Create(Children.Add(name, ChildNameReserved.Instance));
		}

		public override ChildrenCollection ReleaseName(string name)
		{
			return Create(Children.Remove(name));
		}

		public override ChildrenCollection AddChild(ChildRestartInfo childRestartInfo)
		{
			var name = childRestartInfo.Child.Name;
			var newChildren = Children.ContainsKey(name) ? Children.Remove(name).Add(name, childRestartInfo) : Children.Add(name, childRestartInfo);
			return Create(newChildren);
		}

		public override ChildrenCollection AddChild(string name, ChildRestartInfo childRestartInfo)
		{
			var newChildren = Children.ContainsKey(name) ? Children.Remove(name).Add(name, childRestartInfo) : Children.Add(name, childRestartInfo);
			return Create(newChildren);
		}

		public override ChildrenCollection RemoveChild(ActorRef child)
		{
			var name = child.Name;
			if(Children.ContainsKey(name))
			{
				return Create(Children.Remove(name));
			}
			return this;
		}

		protected virtual ChildrenCollection Create(ImmutableDictionary<string, ChildInfo> children)
		{
			return CreateNew(children);
		}

		public static ChildrenCollection CreateNew(ImmutableDictionary<string, ChildInfo> children)
		{
			if(children.IsEmpty) return EmptyChildrenCollection.Instance;
			return new NormalChildrenCollection(children);
		}

		public override IEnumerable<InternalActorRef> GetChildrenRefs()
		{
			return Children.Values.Where(i => i is ChildRestartInfo).Select(c => ((ChildRestartInfo)c).Child);
		}

		public override IEnumerable<ChildRestartInfo> GetChildren()
		{
			return Children.Values.Where(i => i is ChildRestartInfo).Cast<ChildRestartInfo>();
		}

		public override IEnumerable<KeyValuePair<string, ChildRestartInfo>> GetChildrenWithName()
		{
			return Children.Where(kvp => kvp.Value is ChildRestartInfo).Select(kvp => new KeyValuePair<string, ChildRestartInfo>(kvp.Key, (ChildRestartInfo)kvp.Value));
		}

		public override bool TryGetByName(string actorName, out ChildInfo child)
		{
			return Children.TryGetValue(actorName, out child);
		}

		public override bool TryGetByRef(ActorRef actorRef, out ChildRestartInfo child)
		{
			ChildInfo childInfo;
			if(!TryGetByName(actorRef.Name, out childInfo))
			{
				child = null;
				return false;
			}
			child = childInfo as ChildRestartInfo;
			return child != null;
		}

		public override ChildrenCollection IsAboutToTerminate(ActorRef child)
		{
			return new TerminatingChildrenCollection(Children, child);
		}

		public override bool HasChildrenThatAreTerminating()
		{
			return false;
		}
	}

	public sealed class TerminatedChildrenCollection : EmptyChildrenCollection
	{
		public static readonly TerminatedChildrenCollection Instance = new TerminatedChildrenCollection();
		private TerminatedChildrenCollection() { }

		public override ChildrenCollection AddChild(string name, ChildRestartInfo childRestartInfo) { return this; }
		public override ChildrenCollection ReserveName(string name)
		{
			throw new InvalidOperationException(string.Format("Cannot reserve the actor name \"{0}\" as the supervisor has terminated.", name));
		}
		public override bool HasChildrenThatAreTerminating()
		{
			return true;
		}
	}
	public class TerminatingChildrenCollection : NormalChildrenCollection
	{
		private readonly ImmutableHashSet<ActorRef> _terminatingActors;

		public TerminatingChildrenCollection(ImmutableDictionary<string, ChildInfo> children, ActorRef terminatingActor)
			: this(children, ImmutableHashSet.Create(terminatingActor))
		{
			//Intentionally left blank
		}

		protected TerminatingChildrenCollection(ImmutableDictionary<string, ChildInfo> children, ImmutableHashSet<ActorRef> terminatingActors)
			: base(children)
		{
			_terminatingActors = terminatingActors;
		}

		public virtual ChildrenCollection OwnerIsTerminating()
		{
			return new OwnerIsTerminatingChildrenCollection(Children, _terminatingActors);
		}

		public override ChildrenCollection RemoveChild(ActorRef child)
		{
			var remainingTerminationgActors = _terminatingActors.Remove(child);
			var remainingChildren = Children.Remove(child.Name);
			var wasRemoved = Children.Count - remainingChildren.Count > 0;
			if(!wasRemoved) return this;
			if(remainingTerminationgActors.IsEmpty)
				return CreateEmpty(remainingChildren);
			return Create(remainingChildren, remainingTerminationgActors);
		}

		protected override ChildrenCollection Create(ImmutableDictionary<string, ChildInfo> children)
		{
			return Create(children, _terminatingActors);
		}

		protected virtual ChildrenCollection Create(ImmutableDictionary<string, ChildInfo> children, ImmutableHashSet<ActorRef> terminatingActors)
		{
			return new TerminatingChildrenCollection(children, terminatingActors);
		}

		protected virtual ChildrenCollection CreateEmpty(ImmutableDictionary<string, ChildInfo> children)
		{
			return CreateNew(children);
		}

		public override ChildrenCollection IsAboutToTerminate(ActorRef child)
		{
			return Create(Children, _terminatingActors.Add(child));
		}

		public override bool HasChildrenThatAreTerminating()
		{
			return true;
		}
	}

	public class OwnerIsTerminatingChildrenCollection : TerminatingChildrenCollection
	{
		public OwnerIsTerminatingChildrenCollection(ImmutableDictionary<string, ChildInfo> children, ImmutableHashSet<ActorRef> terminatingActors)
			: base(children, terminatingActors)
		{
		}

		protected override ChildrenCollection Create(ImmutableDictionary<string, ChildInfo> children, ImmutableHashSet<ActorRef> terminatingActors)
		{
			return new OwnerIsTerminatingChildrenCollection(children, terminatingActors);
		}

		protected override ChildrenCollection CreateEmpty(ImmutableDictionary<string, ChildInfo> children)
		{
			return TerminatedChildrenCollection.Instance;
		}

		public override ChildrenCollection ReserveName(string name)
		{
			throw new InvalidOperationException(string.Format("Cannot reserve the actor name \"{0}\" as the supervisor is terminating.", name));
		}

		public override ChildrenCollection OwnerIsTerminating()
		{
			return this;
		}
	}

	public interface ChildInfo
	{
	}

	[DebuggerDisplay("[{Child,nq}] {NumberOfRestarts} restarts")]
	public class ChildRestartInfo : ChildInfo
	{
		private readonly InternalActorRef _child;
		private readonly uint _numberOfRestarts = 0;
		private readonly long _restartTimeWindowStartTicks;

		public ChildRestartInfo(InternalActorRef child)
			: this(child, 0, 0)
		{
		}

		private ChildRestartInfo(InternalActorRef child, uint numberOfRestarts, long restartTimeWindowStartTicks)
		{
			_child = child;
			_numberOfRestarts = numberOfRestarts;
			_restartTimeWindowStartTicks = restartTimeWindowStartTicks;
		}

		public InternalActorRef Child { get { return _child; } }
		public uint NumberOfRestarts { get { return _numberOfRestarts; } }
		public long RestartTimeWindowStartTicks { get { return _restartTimeWindowStartTicks; } }

		public ChildRestartInfo CreateUpdate(uint retries, long restartTimeWindowStartTicks)
		{
			return new ChildRestartInfo(_child, retries, restartTimeWindowStartTicks > 0 ? restartTimeWindowStartTicks : 0);
		}
	}

	public interface RestartableChildRestartInfo : ChildInfo
	{
		ActorRef Actor { get; }
		bool RequestRestartPermission(int maxRetriesAllowed, int restartTimeWindowMs);
	}

	public class InternalRestartableChildRestartInfo : RestartableChildRestartInfo
	{
		private ChildRestartInfo _info;
		private bool _isUpdated;

		public InternalRestartableChildRestartInfo(ChildRestartInfo info)
		{
			_info = info;
		}

		public bool IsUpdated
		{
			get { return _isUpdated; }
		}
		
		public ChildRestartInfo Info
		{
			get { return _info; }
		}

		public ActorRef Actor { get { return _info.Child; } }

		public bool RequestRestartPermission(int maxRetriesAllowed, int restartTimeWindowMs)
		{
			if(maxRetriesAllowed >= 0)
			{
				if(maxRetriesAllowed == 0) return false;
				if(restartTimeWindowMs <= 0)
				{
					return IncreaseAndCheckNumberOfRetries(maxRetriesAllowed);
				}
				return CheckWindow(maxRetriesAllowed, (uint)restartTimeWindowMs);
			}
			if(restartTimeWindowMs > 0)
			{
				return CheckWindow(1, (uint)restartTimeWindowMs);
			}
			return true;
		}

		private bool CheckWindow(int maxRetriesAllowed, uint restartTimeWindowMs)
		{
			var now = DateTime.UtcNow.Ticks;
			var windowStart = Info.RestartTimeWindowStartTicks;
			if(windowStart == 0)
				windowStart = now;
			var insideWindow = now - windowStart <= TimeSpan.TicksPerMillisecond * restartTimeWindowMs;
			if(insideWindow)
			{
				return IncreaseAndCheckNumberOfRetries(maxRetriesAllowed, windowStart);
			}
			Update(1, windowStart);
			return true;
		}

		private bool IncreaseAndCheckNumberOfRetries(int maxRetriesAllowed, long windowStart = -1)
		{
			Update(Info.NumberOfRestarts + 1, windowStart);
			return Info.NumberOfRestarts <= maxRetriesAllowed;

		}

		private void Update(uint retries, long windowStart)
		{
			_info = Info.CreateUpdate(retries, windowStart);
			_isUpdated = true;
		}
	}

	class ChildNameReserved : ChildInfo
	{
		public readonly static ChildNameReserved Instance = new ChildNameReserved();
		private ChildNameReserved() { }
	}
}