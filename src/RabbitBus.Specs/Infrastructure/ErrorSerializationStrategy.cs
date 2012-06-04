using System;
using RabbitBus.Configuration;

namespace RabbitBus.Specs.Infrastructure
{
	class ErrorSerializationStrategy : ISerializationStrategy
	{
		public string ContentType
		{
			get { return string.Empty; }
		}

		public string ContentEncoding
		{
			get { return string.Empty; }
		}

		public byte[] Serialize<T>(T message)
		{
			throw new Exception("invalid");
		}

		public T Deserialize<T>(byte[] bytes)
		{
			throw new Exception("invalid");
		}
	}
}