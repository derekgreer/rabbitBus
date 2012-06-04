using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace RabbitBus.Configuration
{
	public class BinarySerializationStrategy : ISerializationStrategy
	{
		public string ContentType
		{
			get { return "application/x-dotnet-serialized-object"; }
		}

		public string ContentEncoding
		{
			get { return string.Empty; }
		}

		public byte[] Serialize<T>(T message)
		{
			using (var stream = new MemoryStream())
			{
				IFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, message);
				return stream.GetBuffer();
			}
		}

		public T Deserialize<T>(byte[] bytes)
		{
			using (var stream = new MemoryStream(bytes))
			{
				IFormatter formatter = new BinaryFormatter();
				object o = formatter.Deserialize(stream);
				return (T) o;
			}
		}
	}
}