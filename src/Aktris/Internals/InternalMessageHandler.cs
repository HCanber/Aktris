using Aktris.Dispatching;
using Aktris.Internals.SystemMessages;

namespace Aktris.Internals
{
	public interface InternalMessageHandler
	{
		void HandleMessage(Envelope envelope);
		void HandleSystemMessage(SystemMessageEnvelope envelope);		
	}
}