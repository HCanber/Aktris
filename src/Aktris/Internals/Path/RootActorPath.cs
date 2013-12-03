using System;
using System.Text;

namespace Aktris.Internals.Path
{
	
	public class RootActorPath : ActorPath, IEquatable<RootActorPath>
	{
		private readonly string _name;

		public RootActorPath(string name)
		{
			_name = name;
		}

		public override string Name { get { return _name; } }

		internal override uint InstanceId { get { return LocalActorRef.UndefinedInstanceId; } }

		public override ActorPath Parent { get { return this; } }

		public override ActorPath Root { get { return this; } }

		public override bool Equals(ActorPath other)
		{
			if(ReferenceEquals(null, other)) return false;
			if(ReferenceEquals(this, other)) return true;
			if(other.GetType() != this.GetType()) return false;
			return Equals((RootActorPath)other);
		}

		public bool Equals(RootActorPath other)
		{
			return string.Equals(_name, other._name);

		}

		public override int CompareTo(ActorPath other)
		{
			var rootActorPath = other as RootActorPath;
			if(rootActorPath != null)
			{
				return String.Compare(_name, other.Name, StringComparison.Ordinal);
			}
			return 1;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return _name.GetHashCode();
			}
		}

		public static bool operator ==(RootActorPath left, RootActorPath right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(RootActorPath left, RootActorPath right)
		{
			return !Equals(left, right);
		}

		protected internal override void AppendDebugString(StringBuilder sb)
		{
			sb.Append(_name);
		}
	}
}