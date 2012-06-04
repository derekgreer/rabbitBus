using System;
using RabbitBus.Configuration;

namespace RabbitBus
{
	public class DefaultSubscriptionConvention : SubscriptionConventionBase
	{
		public override bool ShouldRegister(Type type)
		{
			return type.Name.EndsWith("Message");
		}

		public override string GetExchangeName(Type type)
		{
			return type.Name;
		}

		public override string GetQueueName(Type type)
		{
			return type.Name;
		}
	}
}