using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Aktris.Internals.Helpers;
using Aktris.JetBrainsAnnotations;

namespace Aktris
{
	public class MessageHandlerConfigurator
	{
		private List<Tuple<Type, MessageHandler>> _handlers = new List<Tuple<Type, MessageHandler>>();

		/// <summary>
		/// Registers a handler for incoming messages of the specified type. 
		/// If <paramref name="matches"/>!=<c>null</c> then this must return true before a message is passed to <paramref name="handler"/>.
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <typeparam name="T">The type of the message</typeparam>
		/// <param name="handler">The message handler that is invoked for incoming messages of the specified type <typeparamref name="T"/></param>
		/// <param name="matches">When not <c>null</c> it is used to determine if the message matches.</param>
		public void Receive<T>(Predicate<T> matches, [NotNull] Action<T> handler)
		{
			Receive(handler, matches);
		}

		/// <summary>
		/// Registers a handler for incoming messages of the specified type. 
		/// If <paramref name="matches"/>!=<c>null</c> then this must return true before a message is passed to <paramref name="handler"/>.
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <typeparam name="T">The type of the message</typeparam>
		/// <param name="handler">The message handler that is invoked for incoming messages of the specified type <typeparamref name="T"/></param>
		/// <param name="matches">When not <c>null</c> it is used to determine if the message matches.</param>
		public void Receive<T>([NotNull] Action<T> handler, Predicate<T> matches=null)
		{
			if(handler == null) throw new ArgumentNullException("handler");
			if(matches==null)
				AddReceiver(typeof(T), (o,s) => handler((T)o));
			else
				AddReceiver(typeof(T), (o,s) =>
				{
					var m = (T) o;
					if(matches(m))
					{
						handler(m);
						return true;
					}
					return false;
				});
		}

		/// <summary>
		/// Registers a handler for incoming messages of any of the specified types. 
		/// If <paramref name="matches"/>!=<c>null</c> then it
		/// must return true before a message is passed to <paramref name="handler"/>.
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <typeparam name="T1">One of the expected types of the message</typeparam>
		/// <typeparam name="T2">One of the expected types of the message</typeparam>
		/// <param name="handler">The message handler that is invoked for incoming messages of the specified type <typeparamref name="T1"/></param>
		/// <param name="matches">When not <c>null</c> it is used to determine if the message matches.</param>
		public void Receive<T1, T2>(Predicate<object> matches, [NotNull] Action<object> handler)
		{
			Receive<T1,T2>(handler, matches);
		}

		/// <summary>
		/// Registers a handler for incoming messages of any of the specified types. 
		/// If <paramref name="matches"/>!=<c>null</c> then it
		/// must return true before a message is passed to <paramref name="handler"/>.
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <typeparam name="T1">One of the expected types of the message</typeparam>
		/// <typeparam name="T2">One of the expected types of the message</typeparam>
		/// <param name="handler">The message handler that is invoked for incoming messages of the specified type <typeparamref name="T1"/></param>
		/// <param name="matches">When not <c>null</c> it is used to determine if the message matches.</param>
		public void Receive<T1, T2>([NotNull] Action<object> handler, Predicate<object> matches = null)
		{
			if(handler == null) throw new ArgumentNullException("handler");
			Receive(new[] { typeof(T1), typeof(T2) },(o,s)=> handler(o), matches);
		}


		/// <summary>
		/// Registers a handler for incoming messages of any of the specified types. 
		/// If <paramref name="matches"/>!=<c>null</c> then it
		/// must return true before a message is passed to <paramref name="handler"/>.
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <typeparam name="T1">One of the expected types of the message</typeparam>
		/// <typeparam name="T2">One of the expected types of the message</typeparam>
		/// <typeparam name="T3">One of the expected types of the message</typeparam>
		/// <param name="handler">The message handler that is invoked for incoming messages of the specified type <typeparamref name="T1"/></param>
		/// <param name="matches">When not <c>null</c> it is used to determine if the message matches.</param>
		public void Receive<T1, T2, T3>(Predicate<object> matches, [NotNull] Action<object> handler)
		{
			Receive<T1,T2,T3>(handler, matches);
		}

		/// <summary>
		/// Registers a handler for incoming messages of any of the specified types. 
		/// If <paramref name="matches"/>!=<c>null</c> then it
		/// must return true before a message is passed to <paramref name="handler"/>.
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <typeparam name="T1">One of the expected types of the message</typeparam>
		/// <typeparam name="T2">One of the expected types of the message</typeparam>
		/// <typeparam name="T3">One of the expected types of the message</typeparam>
		/// <param name="handler">The message handler that is invoked for incoming messages of the specified type <typeparamref name="T1"/></param>
		/// <param name="matches">When not <c>null</c> it is used to determine if the message matches.</param>
		public void Receive<T1, T2, T3>([NotNull] Action<object> handler, Predicate<object> matches = null)
		{
			if(handler == null) throw new ArgumentNullException("handler");
			Receive(new[] { typeof(T1), typeof(T2), typeof(T3) }, (o, s) => handler(o), matches);
		}


		/// <summary>
		/// Registers a handler for incoming messages of the specified type. 
		/// If <paramref name="matches"/>!=<c>null</c> then this must return true before a message is passed to <paramref name="handler"/>.
		/// <typeparamref name="T"/>.
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <typeparam name="T">The type of the message</typeparam>
		/// <param name="handler">The message handler that is invoked for incoming messages of the specified type <typeparamref name="T"/>.</param>
		/// <param name="matches">When not <c>null</c> it is used to determine if the message matches.</param>
		public void Receive<T>(Predicate<T> matches, [NotNull] Action<T, SenderActorRef> handler)
		{
			Receive<T>(handler, matches);
		}

		/// <summary>
		/// Registers a handler for incoming messages of the specified type. 
		/// If <paramref name="matches"/>!=<c>null</c> then this must return true before a message is passed to <paramref name="handler"/>.
		/// <typeparamref name="T"/>.
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <typeparam name="T">The type of the message</typeparam>
		/// <param name="handler">The message handler that is invoked for incoming messages of the specified type <typeparamref name="T"/>.</param>
		/// <param name="matches">When not <c>null</c> it is used to determine if the message matches.</param>
		public void Receive<T>([NotNull] Action<T, SenderActorRef> handler, Predicate<T> matches = null)
		{
			if(handler == null) throw new ArgumentNullException("handler");
			if(matches == null)
				AddReceiver(typeof(T), (o, s) => handler((T)o,s));
			else
				AddReceiver(typeof(T), (o, s) =>
				{
					var m = (T)o;
					if(matches(m))
					{
						handler(m,s);
						return true;
					}
					return false;
				});
		}

		/// <summary>
		/// Registers a handler for incoming messages of any of the specified types. 
		/// If <paramref name="matches"/>!=<c>null</c> then it
		/// must return true before a message is passed to <paramref name="handler"/>.
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <typeparam name="T1">One of the expected types of the message</typeparam>
		/// <typeparam name="T2">One of the expected types of the message</typeparam>
		/// <param name="handler">The message handler that is invoked for incoming messages of the specified type <typeparamref name="T1"/></param>
		/// <param name="matches">When not <c>null</c> it is used to determine if the message matches.</param>
		public void Receive<T1, T2>(Predicate<object> matches, [NotNull] Action<object, SenderActorRef> handler)
		{
			Receive<T1, T2>(handler, matches);
		}

		/// <summary>
		/// Registers a handler for incoming messages of any of the specified types. 
		/// If <paramref name="matches"/>!=<c>null</c> then it
		/// must return true before a message is passed to <paramref name="handler"/>.
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <typeparam name="T1">One of the expected types of the message</typeparam>
		/// <typeparam name="T2">One of the expected types of the message</typeparam>
		/// <param name="handler">The message handler that is invoked for incoming messages of the specified type <typeparamref name="T1"/></param>
		/// <param name="matches">When not <c>null</c> it is used to determine if the message matches.</param>
		public void Receive<T1, T2>([NotNull] Action<object, SenderActorRef> handler, Predicate<object> matches = null)
		{
			if(handler == null) throw new ArgumentNullException("handler");
			Receive(new[] { typeof(T1), typeof(T2)}, handler, matches);	
		}

		/// <summary>
		/// Registers a handler for incoming messages of any of the specified types. 
		/// If <paramref name="matches"/>!=<c>null</c> then it
		/// must return true before a message is passed to <paramref name="handler"/>.
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <typeparam name="T1">One of the expected types of the message</typeparam>
		/// <typeparam name="T2">One of the expected types of the message</typeparam>
		/// <typeparam name="T3">One of the expected types of the message</typeparam>
		/// <param name="handler">The message handler that is invoked for incoming messages of the specified type <typeparamref name="T1"/></param>
		/// <param name="matches">When not <c>null</c> it is used to determine if the message matches.</param>
		public void Receive<T1, T2, T3>(Predicate<object> matches, [NotNull] Action<object, SenderActorRef> handler)
		{
			Receive<T1,T2,T3>(handler, matches);
		}

		/// <summary>
		/// Registers a handler for incoming messages of any of the specified types. 
		/// If <paramref name="matches"/>!=<c>null</c> then it
		/// must return true before a message is passed to <paramref name="handler"/>.
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <typeparam name="T1">One of the expected types of the message</typeparam>
		/// <typeparam name="T2">One of the expected types of the message</typeparam>
		/// <typeparam name="T3">One of the expected types of the message</typeparam>
		/// <param name="handler">The message handler that is invoked for incoming messages of the specified type <typeparamref name="T1"/></param>
		/// <param name="matches">When not <c>null</c> it is used to determine if the message matches.</param>
		public void Receive<T1, T2, T3>([NotNull] Action<object, SenderActorRef> handler, Predicate<object> matches = null)
		{
			if(handler == null) throw new ArgumentNullException("handler");
			Receive(new[] {typeof(T1), typeof(T2), typeof(T3)}, handler, matches);	
		}

		/// <summary>
		/// Registers a handler for incoming messages of any of the specified types. 
		/// If <paramref name="matches"/>!=<c>null</c> then it
		/// must return true before a message is passed to <paramref name="handler"/>.
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <param name="messageTypes">The collection of handled types</param>
		/// <param name="handler">The message handler that is invoked for incoming messages of the specified type <typeparamref name="T1"/></param>
		/// <param name="matches">When not <c>null</c> it is used to determine if the message matches.</param>
		public void Receive(IEnumerable<Type> messageTypes, Predicate<object> matches, [NotNull] Action<object, SenderActorRef> handler)
		{
			Receive(messageTypes, handler, matches);
		}

		/// <summary>
		/// Registers a handler for incoming messages of any of the specified types. 
		/// If <paramref name="matches"/>!=<c>null</c> then it
		/// must return true before a message is passed to <paramref name="handler"/>.
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <param name="messageTypes">The collection of handled types</param>
		/// <param name="handler">The message handler that is invoked for incoming messages of the specified type <typeparamref name="T1"/></param>
		/// <param name="matches">When not <c>null</c> it is used to determine if the message matches.</param>
		public void Receive(IEnumerable<Type> messageTypes, [NotNull] Action<object, SenderActorRef> handler, Predicate<object> matches = null)
		{
			if(matches == null)
			{
				messageTypes.ForEach(t=>AddReceiver(t,handler));
			}
			else
			{
				MessageHandler matchHandler = (o, s) =>
				{
					if(matches(o))
					{
						handler(o, s);
						return true;
					}
					return false;
				};				
				messageTypes.ForEach(t => _handlers.Add(Tuple.Create(t,matchHandler)));
			}
		}
		/// <summary>
		/// Forwards all incoming messages of the specified type to the specified actor.
		/// <typeparamref name="T"/>.
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <typeparam name="T">The type of the message</typeparam>
		/// <param name="receiver">The recipient of all forwarded messages</param>
		public void ReceiveAndForward<T>([NotNull] ActorRef receiver)
		{
			if(receiver == null) throw new ArgumentNullException("receiver");
			AddReceiver(typeof(T), (m, sender) => receiver.Send(m, sender));
		}

		/// <summary>
		/// Registers a handler for incoming messages of any type.
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <param name="handler">The message handler that is invoked for all</param>
		public void ReceiveAny([NotNull] Action<object,SenderActorRef> handler)
		{
			if(handler == null) throw new ArgumentNullException("handler");
			AddReceiver(typeof(object), handler);
		}

		/// <summary>
		/// Registers a handler for incoming messages of any type.
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <param name="handler">The message handler that is invoked for all</param>
		public void ReceiveAny([NotNull] Action<object> handler)
		{
			if(handler == null) throw new ArgumentNullException("handler");
			AddReceiver(typeof(object), handler);
		}


		/// <summary>
		/// Forwards all incoming messages oto the specified actor.
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <param name="receiver">The recipient of all incoming messages</param>
		public void ReceiveAnyAndForward([NotNull] ActorRef receiver)
		{
			if(receiver == null) throw new ArgumentNullException("receiver");
			AddReceiver(typeof(object), (m,sender) => receiver.Send(m, sender));
		}

		//protected void ReceiveAnyAndForwardToDeadLetters()
		//{
		//	AddReceiver(typeof(object), m => Context.System.DeadLetters.Forward(m, Context.Sender));
		//}

		public void AddReceiver(Type type, Action<object> handler)
		{
			AddReceiver(type, (m,sender) => { handler(m); return true; });
		}

		public void AddReceiver(Type type, Action<object,SenderActorRef> handler)
		{
			AddReceiver(type, (m, sender) => { handler(m,sender); return true; });
		}


		public void AddReceiver(Type type, Func<object, bool> handler)
		{
			_handlers.Add(Tuple.Create<Type, MessageHandler>(type,(m,sender)=>handler(m)));
		}

		public void AddReceiver(Type type, MessageHandler handler)
		{
			_handlers.Add(Tuple.Create<Type, MessageHandler>(type, handler));
		}


		public MessageHandler CreateMessageHandler()
		{
			var handlers = _handlers;
			_handlers = null;
			return CreateMessageHandler(handlers);
		}

		public void CopyFrom(MessageHandlerConfigurator otherConfigurator)
		{
			_handlers.AddRange(otherConfigurator._handlers);
		}

		protected static MessageHandler CreateMessageHandler(IEnumerable<Tuple<Type, MessageHandler>> handlers)
		{
			var internalHandlers = new List<Tuple<Type, MessageHandler>>(handlers);
			return (msg,sender) =>
			{
				foreach(var tuple in internalHandlers)
				{
					if(tuple.Item1.IsInstanceOfType(msg))
					{
						var wasHandled = tuple.Item2(msg,sender);
						if(wasHandled)
						{
							return true;
						}
					}
				}
				return false;
			};
		}
	}
}