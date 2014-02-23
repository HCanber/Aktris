using System;

namespace Aktris
{


	///<summary>Classes for passing status back to the sender.
	///Used for internal ACKing protocol. But exposed as utility class for user-specific ACKing protocols as well.</summary>
	public class Status
	{
		/// <summary>
		/// This class/message type is preferably used to indicate success of some operation performed.
		/// </summary>
		public class Success : Status
		{
			private readonly object _status;

			public Success(object status)
			{
				_status = status;
			}

			public object Status { get { return _status; } }
		}

		//This class/message type is preferably used to indicate failure of some operation performed.
		//As an example, it is used to signal failure with AskSupport is used (ask/?).
		public class Failure : Status
		{
			private readonly Exception _exception;

			public Failure(Exception exception) { _exception = exception; }

			public Exception Exception { get { return _exception; } }
		}
	}
}