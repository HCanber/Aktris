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
		public abstract bool TryGetByRef(ActorRef actorRef, out ChildInfo info);
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

		public override bool TryGetByRef(ActorRef actorRef, out ChildInfo child)
		{
			child = null;
			return false;
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

		public static ChildrenCollection Create(ImmutableDictionary<string, ChildInfo> children)
		{
			if(children.IsEmpty) return EmptyChildrenCollection.Instance;
			return new NormalChildrenCollection(children);
		}

		public override IEnumerator<InternalActorRef> GetEnumerator()
		{
			return _children.Values.Where(i => i is ChildRestartInfo).Select(i => ((ChildRestartInfo)i).Child).GetEnumerator();
		}

		public override bool TryGetByRef(ActorRef actorRef, out ChildInfo child)
		{
			return _children.TryGetValue(actorRef.Name, out child);
		}
	}
	public interface ChildInfo { }

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