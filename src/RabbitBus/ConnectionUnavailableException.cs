using System;
using System.Runtime.Serialization;

namespace RabbitBus
{
	[Serializable]
	public class ConnectionUnavailableException : Exception
	{
		public ConnectionUnavailableException()
		{
		}

		public ConnectionUnavailableException(string message) : base(message)
		{
		}

		public ConnectionUnavailableException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ConnectionUnavailableException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}