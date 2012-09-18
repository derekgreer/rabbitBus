using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitBus.Configuration;
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
				string error = "An error occurred attempting to serialize the provided message: " + e.Message;
				Logger.Current.Write(new LogEntry {Message = error, Severity = TraceEventType.Error});
				throw new SerializationException(error, e);
			}
		}

		public T Deserialize<T>(byte[] bytes)
		{
			try
			{
				string message = Encoding.UTF8.GetString(bytes);
				return JsonConvert.DeserializeObject<T>(message, GetJsonSettings());
			}
			catch (Exception e)
			{
				string error = "An error occurred attempting to deserialize the provided data: " + e.Message;
				Logger.Current.Write(new LogEntry {Message = error, Severity = TraceEventType.Error});
				throw new SerializationException(error, e);
			}
		}

		JsonSerializerSettings GetJsonSettings()
		{
			return new JsonSerializerSettings
			       	{
			       		ContractResolver = new DefaultContractResolver
			       		                   	{
			       		                   		DefaultMembersSearchFlags =
			       		                   			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
			       		                   	}
			       	};
		}
	}
}