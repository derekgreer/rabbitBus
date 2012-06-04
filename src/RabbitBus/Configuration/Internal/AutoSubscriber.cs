using System;
using System.Linq;
using System.Reflection;

namespace RabbitBus.Configuration.Internal
{
	class AutoSubscriber
	{
		public void Subscribe(IConfigurationModel configurationModel, IAutoSubscriptionModel autoSubscriptionModel)
		{
			foreach (Assembly assembly in autoSubscriptionModel.Assemblies)
			{
				foreach (Type type in assembly.GetTypes())
				{
					foreach (ISubscriptionConvention convention in autoSubscriptionModel.Conventions)
					{
						if (convention.ShouldRegister(type))
						{
							var consumeInfo = new ConsumeInfo
							                  	{
							                  		ExchangeName = convention.GetExchangeName(type),
							                  		ExchangeType = convention.GetExchangeType(type),
							                  		QueueName = convention.GetQueueName(type),
							                  		IsAutoAcknowledge = convention.IsAutoAcknowledge(type),
							                  		DefaultRoutingKey = convention.GetDefaultRouteKey(type),
							                  		Exclusive = convention.IsExclusive(type),
							                  		IsExchangeAutoDelete = convention.IsAutoDeleteExchange(type),
							                  		IsExchangeDurable = convention.IsDurableExchange(type),
							                  		IsQueueAutoDelete = convention.IsAutoDeleteQueue(type),
							                  		IsQueueDurable = convention.IsDurableQueue(type),
							                  	};

							ISerializationStrategy serializationStrategy = convention.GetSerializationStrategy(type);
							if (serializationStrategy != null)
							{
								consumeInfo.SerializationStrategy = serializationStrategy;
							}

							Action<IErrorContext> errorCallback = convention.GetErrorCallback(type);
							if (errorCallback != null)
							{
								consumeInfo.ErrorCallback = errorCallback;
							}

							configurationModel.ConsumeRouteConfiguration.AddPolicy<MappingRouteInfoLookupStrategy<IConsumeInfo>>(type,
							                                                                                                     consumeInfo);

							foreach (Assembly handlerProspectAssembly in autoSubscriptionModel.Assemblies)
							{
								foreach (
									Type handler in handlerProspectAssembly.GetTypes().Where(t => typeof (IMessageHandler).IsAssignableFrom(t)))
								{
									foreach (IHandlerConvention handlerConvention in autoSubscriptionModel.HandlerConventions)
									{
										if (handlerConvention.ShouldHandle(type, handler))
										{
											configurationModel.AutoSubscriptions.Add(new AutoSubscription(type, handler));
											break;
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}
}