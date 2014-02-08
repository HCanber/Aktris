using System;
using System.Collections.Concurrent;
using Aktris.Internals.Helpers;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals.Logging
{
	public class LookupClassification<TEvent, TClassifier, TSubscriber>
	{
		private readonly Func<TEvent, TClassifier> _classify;
		private readonly Action<TEvent, TSubscriber> _publish;
		//TODO: Add SubchannelClassification, which means that if you listen to a Class, you'll receive any message that is of that type or a subtype.
		private readonly ConcurrentDictionary<TClassifier, ConcurrentSet<TSubscriber>> _subscribersByClassifier = new ConcurrentDictionary<TClassifier, ConcurrentSet<TSubscriber>>();

		public LookupClassification([NotNull] Func<TEvent, TClassifier> classify, [NotNull] Action<TEvent, TSubscriber> publish)
		{
			if(classify == null) throw new ArgumentNullException("classify");
			if(publish == null) throw new ArgumentNullException("publish");
			_classify = classify;
			_publish = publish;
		}

		public bool Subscribe(TSubscriber subscriber, TClassifier to)
		{
			var subscribers = _subscribersByClassifier.GetOrAdd(to, _ => new ConcurrentSet<TSubscriber>());
			return subscribers.TryAdd(subscriber);
		}

		public bool Unsubscribe(TSubscriber subscriber)
		{
			var wasRemoved = false;
			foreach(var kvp in _subscribersByClassifier)
			{
				wasRemoved |= kvp.Value.TryRemove(subscriber);
			}
			return wasRemoved;
		}

		public bool Unsubscribe(TSubscriber subscriber, TClassifier from)
		{
			ConcurrentSet<TSubscriber> subscribers;

			if(_subscribersByClassifier.TryGetValue(from, out subscribers))
			{
				var wasRemoved = subscribers.TryRemove(subscriber);
				if(wasRemoved && subscribers.Count == 0)
				{
					//TODO: Remove subscribers from _subscribersByClassifier
				}
				return wasRemoved;
			}
			return false;
		}

		public void Publish(TEvent @event)
		{
			var classifier = _classify(@event);

			ConcurrentSet<TSubscriber> subscribers;
			if(_subscribersByClassifier.TryGetValue(classifier, out subscribers))
			{
				subscribers.ForEach(subscriber => _publish(@event, subscriber));
			}
		}
	}
}