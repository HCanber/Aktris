using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Aktris.Exceptions;

namespace Aktris.Internals.Children
{
	public abstract class ChildrenCollection : IImmutableEnumerable<InternalActorRef>
	{
		public abstract ChildrenCollection ReserveName(string name);
		public abstract ChildrenCollection ReleaseName(string name);
		public abstract IEnumerator<InternalActorRef> GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		public abstract bool TryGetByRef(ActorRef actorRef, out ChildRestartInfo info);
		public abstract bool TryGetByName(string actorName, out ChildInfo info);
		public abstract ChildrenCollection AddOrUpdate(string name, ChildRestartInfo childRestartInfo);
	}

	public class EmptyChildrenCollection : ChildrenCollection
	{
		public static readonly EmptyChildrenCollection Instance = new EmptyChildrenCollection();
		private EmptyChildrenCollection() { }
		public override ChildrenCollection ReserveName(string name)
		{
			return NormalChildrenCollection.Create(ImmutableDictionary<string, ChildInfo>.Empty.Add(name, ChildNameReserved.Instance));
		}

		public override ChildrenCollection ReleaseName(string name)
		{
			return this;
		}

		public override IEnumerator<InternalActorRef> GetEnumerator()
		{
			yield break;
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

		public override ChildrenCollection AddOrUpdate(string name, ChildRestartInfo childRestartInfo)
		{
			return NormalChildrenCollection.Create(ImmutableDictionary<string, ChildInfo>.Empty.Add(name, childRestartInfo));			
		}
	}

	public class NormalChildrenCollection : ChildrenCollection
	{
		private readonly ImmutableDictionary<string, ChildInfo> _children;

		private NormalChildrenCollection(ImmutableDictionary<string, ChildInfo> children)
		{
			_children = children;
		}

		public override ChildrenCollection ReserveName(string name)
		{
			if(_children.ContainsKey(name))
				throw new InvalidActorNameException(string.Format("Actor name \"{0}\" is not unique!", name));

			return new NormalChildrenCollection(_children.Add(name, ChildNameReserved.Instance));
		}

		public override ChildrenCollection ReleaseName(string name)
		{
			return Create(_children.Remove(name));
		}

		public override ChildrenCollection AddOrUpdate(string name, ChildRestartInfo childRestartInfo)
		{
			var newChildren= _children.ContainsKey(name) ? _children.Remove(name).Add(name, childRestartInfo) : _children.Add(name, childRestartInfo);
			return Create(newChildren);
		}

		public static ChildrenCollection Create(ImmutableDictionary<string, ChildInfo> children)
		{
			if(children.IsEmpty) return EmptyChildrenCollection.Instance;
			return new NormalChildrenCollection(children);
		}

		public override IEnumerator<InternalActorRef> GetEnumerator()
		{
			return _children.Values.Where(i => i is ChildRestartInfo).Select(i => ((ChildRestartInfo)i).Child).GetEnumerator();
		}

		public override bool TryGetByName(string actorName, out ChildInfo child)
		{
			return _children.TryGetValue(actorName, out child);
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
	}
	public interface ChildInfo {
	}

	public class ChildRestartInfo : ChildInfo
	{
		private readonly InternalActorRef _child;

		public ChildRestartInfo(InternalActorRef child)
		{
			_child = child;
		}

		public InternalActorRef Child { get { return _child; } }
	}

	class ChildNameReserved : ChildInfo
	{
		public readonly static ChildNameReserved Instance = new ChildNameReserved();
		private ChildNameReserved() { }
	}
}