using System.Collections.Immutable;
using Aktris.Exceptions;

namespace Aktris.Internals.Children
{
	public abstract class ChildrenCollection
	{
		public abstract ChildrenCollection ReserveName(string name);
		public abstract ChildrenCollection ReleaseName(string name);
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
	}
	public interface ChildInfo { }

	class ChildNameReserved : ChildInfo
	{
		public readonly static ChildNameReserved Instance = new ChildNameReserved();
		private ChildNameReserved() { }
	}
}