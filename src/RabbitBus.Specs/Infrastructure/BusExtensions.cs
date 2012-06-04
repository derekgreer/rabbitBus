using System.Collections.Generic;

namespace RabbitBus.Specs.Infrastructure
{
	public static class BusExtensions
	{
		public static IEnumerable<TMessage> GetMessages<TMessage>(this IBus bus)
		{
			IEnumerable<TMessage> messages = null;

			using (IConsumerContext<TMessage> context = bus.CreateConsumerContext<TMessage>())
			{
				foreach (var messageContext in Enumerator.Enumerate(context.GetMessage))
				{
					if (messageContext.AcceptanceRequired)
						messageContext.AcceptMessage();

					yield return messageContext.Message;
				}
			}
		}
	}
}