using System;
using System.Diagnostics;
using System.Text;
using Aktris.Internals.Path;
using Aktris.JetBrainsAnnotations;

namespace Aktris
{
	[DebuggerDisplay("{GetStringForDebug(),nq}")]
	public abstract class ActorPath : IComparable<ActorPath>, IEquatable<ActorPath>
	{
		/// <summary>The name of the actor that this path refers to.</summary>
		public abstract string Name { get; }
		
		/// <summary>Unique identifier of the actor. Used for distinguishing different incarnations of actors with same path (name elements).</summary>
		internal abstract uint InstanceId { get; }

		/// <summary>The path for the parent actor.</summary>
		public abstract ActorPath Parent { get; }

		/// <summary>The path for the root.</summary>
		public abstract ActorPath Root { get; }

		public abstract bool Equals(ActorPath other);

		public abstract int CompareTo(ActorPath other);

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj)) return false;
			if(ReferenceEquals(this, obj)) return true;
			if(obj.GetType() != this.GetType()) return false;
			return Equals((ActorPath)obj);
		}

		public static bool operator ==(ActorPath left, ActorPath right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(ActorPath left, ActorPath right)
		{
			return !Equals(left, right);
		}

		[UsedImplicitly]
		private string GetStringForDebug()
		{
			var sb = new StringBuilder();
			AppendDebugString(sb);
			return sb.ToString();
		}

		protected internal abstract void AppendDebugString(StringBuilder sb);
	}
}