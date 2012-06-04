using System.Collections;

namespace RabbitBus
{
	public class MessageInfo
	{
		public object Message { get; set; }

		public string RoutingKey { get; set; }

		public IDictionary Headers { get; set; }
	}
}