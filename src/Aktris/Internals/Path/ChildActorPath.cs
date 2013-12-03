using System;
using System.Text;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals.Path
{
	public class ChildActorPath : ActorPath
	{
		private readonly ActorPath _parent;
		private readonly string _name;
		private readonly uint _instanceId;

		public ChildActorPath([NotNull] ActorPath parent, [NotNull] string name, uint instanceId)
		{
			if(parent == null) throw new ArgumentNullException("parent");
			if(name == null) throw new ArgumentNullException("name");
			if(name.Length == 0) throw new ArgumentException("Name must be specified", "name");
			if(name.IndexOf('/') >= 0) throw new ArgumentException(string.Format("/ is a path separator and is not legal in ActorPath names: [{0}]", name));
			if(name.IndexOf('#') >= 0) throw new ArgumentException(string.Format("# is a fragment separator and is not legal in ActorPath names: [{0}]", name));

			_name = name;
			_instanceId = instanceId;
			_parent = parent;
		}

		public override string Name { get { return _name; } }

		internal override uint InstanceId { get { return _instanceId; } }

		public override ActorPath Parent { get { return _parent; } }

		public override ActorPath Root { get { return _parent.Root; } }


		public override bool Equals(ActorPath other)
		{
			if(ReferenceEquals(null, other)) return false;
			ActorPath left = this;
			ActorPath right = other;
			while(true)
			{
				if(ReferenceEquals(left, right)) return true;

				var rightRoot = right as RootActorPath;
				var leftRoot = left as RootActorPath;
				var rightIsRoot = rightRoot != null;
				if(rightIsRoot)
				{
					var leftIsAlsoRoot = leftRoot != null;
					return leftIsAlsoRoot && rightRoot.Equals(leftRoot);
				}
				//right was not root
				var onlyLeftIsRoot = leftRoot != null;
				if(onlyLeftIsRoot) return false;

				//Both are children
				if(!string.Equals(left.Name, right.Name, StringComparison.Ordinal)) return false;
				left = left.Parent;
				right = right.Parent;
			}
		}

		public override int CompareTo(ActorPath other)
		{
			if(ReferenceEquals(null, other)) return 1;
			ActorPath left = this;
			ActorPath right = other;
			while(true)
			{
				if(ReferenceEquals(left, right)) return 0;
				var rightRoot = right as RootActorPath;
				var leftRoot = left as RootActorPath;
				if(rightRoot != null)
				{
					return -1*rightRoot.CompareTo(left);
				}
				if(leftRoot != null) return 1;
				//Both are children
				var nameComparison = string.CompareOrdinal(left.Name, right.Name);
				if(nameComparison != 0) return nameComparison;
				left = left.Parent;
				right = right.Parent;
			}
		}
		public override int GetHashCode()
		{
			return (_parent.GetHashCode() * 397) ^ _name.GetHashCode();
		}

		protected internal override void AppendDebugString(StringBuilder sb)
		{
			AppendDebugStringWithoutId(sb);
			sb.Append('#').Append(_instanceId);
		}

		private void AppendDebugStringWithoutId(StringBuilder sb)
		{
			var parentAsChild = _parent as ChildActorPath;
			if(parentAsChild != null)
			{
				parentAsChild.AppendDebugStringWithoutId(sb);
				sb.Append('/');
			}
			else
			{
				_parent.AppendDebugString(sb);
			}
			sb.Append(_name);
		}
	}
}