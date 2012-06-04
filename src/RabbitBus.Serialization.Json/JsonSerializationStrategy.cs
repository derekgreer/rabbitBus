using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using RabbitBus.Configuration;
using RabbitBus.Configuration.Internal;
using RabbitBus.Logging;

namespace RabbitBus.Serialization.Json
{
	public class JsonSerializationStrategy : ISerializationStrategy
	{
		public string ContentType
		{
			get { return "application/json; charset=utf-8"; }
		}

		public string ContentEncoding
		{
			get { return "utf-8"; }
		}

		public byte[] Serialize<T>(T message)
		{
			try
			{
				string json = JsonConvert.SerializeObject(message);
				return Encoding.UTF8.GetBytes(json);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				var error = "An error occurred attempting to serialize the provided message: " + e.Message;
				Logger.Current.Write(new LogEntry {Message = error, Severity = TraceEventType.Error});
				throw new SerializationException(error, e);
			}
		}

		public T Deserialize<T>(byte[] bytes)
		{
			try
			{
				var message = Encoding.UTF8.GetString(bytes);
				return JsonConvert.DeserializeObject<T>(message);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				var error = "An error occurred attempting to deserialize the provided data: " + e.Message;
				Logger.Current.Write(new LogEntry {Message = error, Severity = TraceEventType.Error});
				throw new SerializationException(error, e);
			}
		}
	}
}