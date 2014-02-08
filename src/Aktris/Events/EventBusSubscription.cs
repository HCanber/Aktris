namespace Aktris.Events
{
	public interface EventBusSubscription<in TClassifier, in TSubscriber>
	{
		/// <summary>Attempts to register the subscriber to the specified Classifier. </summary>
		/// <returns><c>true</c> if successful; <c>false</c> otherwise, already subscribed to that Classifier, or some other reason.</returns>
		bool Subscribe(TSubscriber subscriber, TClassifier to);

		/// <summary>Attempts to deregister the subscriber from the specified Classifier. </summary>
		/// <returns><c>true</c> if successful; <c>false</c> otherwise, already subscribed to that Classifier, or some other reason.</returns>
		bool Unsubscribe(TSubscriber subscriber);

		/// <summary>Attempts to deregister the subscriber from all subscribed Classifiers. </summary>
		bool Unsubscribe(TSubscriber subscriber, TClassifier to);
	}
}