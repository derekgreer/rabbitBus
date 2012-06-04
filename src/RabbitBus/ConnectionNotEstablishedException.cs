using System;
using System.Runtime.Serialization;

namespace RabbitBus
{
	[Serializable]
	public class ConnectionNotEstablishedException : Exception
	{
		public ConnectionNotEstablishedException()
		{
		}

		public ConnectionNotEstablishedException(string message) : base(message)
		{
		}

		public ConnectionNotEstablishedException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ConnectionNotEstablishedException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}