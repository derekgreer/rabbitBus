using System;
using System.Collections;
using System.Collections.Generic;

namespace RabbitBus
{
	public static class BusExtensions
	{
		public static void Publish<TMessage>(this IBus bus, TMessage message)
		{
			bus.Publish(message, MessageProperties.Empty);
		}

		public static void Publish<TMessage>(this IBus bus, TMessage message, IDictionary<string, object> headers)
		{
			bus.Publish(message, new MessageProperties { Headers = headers });
		}

		public static void Publish<TMessage>(this IBus bus, TMessage message, string routingKey)
		{
			bus.Publish(message, new MessageProperties { RoutingKey = routingKey });
		}

		public static void Publish<TRequestMessage, TReplyMessage>(this IBus bus, TRequestMessage requestMessage, Action<IMessageContext<TReplyMessage>> action)
		{
			bus.Publish(requestMessage, MessageProperties.Empty, action, TimeSpan.MinValue);
		}

		public static void Publish<TRequestMessage, TReplyMessage>(this IBus bus, TRequestMessage requestMessage, MessageProperties messageProperties, Action<IMessageContext<TReplyMessage>> action)
		{
			bus.Publish(requestMessage, messageProperties, action, TimeSpan.MinValue);
		}

		public static void Publish<TRequestMessage, TReplyMessage>(this IBus bus, TRequestMessage requestMessage, Action<IMessageContext<TReplyMessage>> action, TimeSpan callbackTimeout)
		{
			bus.Publish(requestMessage, MessageProperties.Empty, action, callbackTimeout);
		}

		public static void Unsubscribe<TMessage>(this IBus bus)
		{
			bus.Unsubscribe<TMessage>(MessageProperties.Empty);
		}

		public static void Unsubscribe<TMessage>(this IBus bus, string routingKey)
		{
			bus.Unsubscribe<TMessage>(new MessageProperties { RoutingKey = routingKey });
		}

		public static void Unsubscribe<TMessage>(this IBus bus, IDictionary<string, object> headers)
		{
			bus.Unsubscribe<TMessage>(new MessageProperties { Headers = headers });
		}

		public static void Subscribe<TMessage>(this IBus bus, Action<IMessageContext<TMessage>> action)
		{
			bus.Subscribe(action, MessageProperties.Empty);
		}

		public static void Subscribe<TMessage>(this IBus bus, Action<IMessageContext<TMessage>> action, string routingKey)
		{
			bus.Subscribe(action, new MessageProperties { RoutingKey = routingKey });
		}

		public static void Subscribe<TMessage>(this IBus bus, Action<IMessageContext<TMessage>> action, IDictionary<string, object> headers)
		{
			bus.Subscribe(action, new MessageProperties { Headers = headers });
		}
	}
}