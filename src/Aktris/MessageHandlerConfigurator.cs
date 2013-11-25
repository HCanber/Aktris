using System;
using System.Collections.Generic;
using System.Linq;
using Aktris.JetBrainsAnnotations;

namespace Aktris
{
	public class MessageHandlerConfigurator
	{
		private List<Tuple<Type, MessageHandler>> _handlers = new List<Tuple<Type, MessageHandler>>();

		/// <summary>
		/// Registers a handler for incoming messages of the specified type 
		/// <typeparamref name="T"/>.
		/// <remarks>This may only be called from the constructor.</remarks>
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <typeparam name="T">The type of the message</typeparam>
		/// <param name="handler">The message handler that is invoked for incoming messages of the specified type <typeparamref name="T"/></param>
		public void Receive<T>([NotNull] Action<T> handler)
		{
			if(handler == null) throw new ArgumentNullException("handler");
			AddReceiver(typeof(T), o => handler((T)o));
		}

		/// <summary>
		/// Registers forwarding all incoming messages of the specified type to the specified actor.
		/// <typeparamref name="T"/>.
		/// <remarks>This may only be called from the constructor.</remarks>
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <typeparam name="T">The type of the message</typeparam>
		/// <param name="receiver">The recipient of all incoming messages of typ <typeparamref name="T"/></param>
		public void ReceiveAndForward<T>([NotNull] ActorRef receiver)
		{
			if(receiver == null) throw new ArgumentNullException("receiver");
			AddReceiver(typeof(T), (m, sender) => receiver.Send(m, sender));
		}

		/// <summary>
		/// Registers a handler for incoming messages of any type.
		/// <remarks>This may only be called from the constructor.</remarks>
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <param name="handler">The message handler that is invoked for all</param>
		public void ReceiveAny([NotNull] Action<object> handler)
		{
			if(handler == null) throw new ArgumentNullException("handler");
			AddReceiver(typeof(object), handler);
		}


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

		public void AddReceiver(Type type, Action<object,ActorRef> handler)
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