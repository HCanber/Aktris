﻿using System;

namespace Aktris
{
	// ReSharper disable once InconsistentNaming
	public interface ActorRef	//TODO : IComparable<ActorRef>, IEquatable<ActorRef>
	{
		string Name { get; }
		void Send(object message, ActorRef sender);
	}
}