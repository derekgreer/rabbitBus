using System;
using System.Reflection;

namespace RabbitBus.Configuration.Internal
{
	class AutoConfigurator
	{
		public void Configure(IConfigurationModel configurationModel, IAutoConfigurationModel autoConfigurationModel)
		{
			foreach (Assembly assembly in autoConfigurationModel.Assemblies)
			{
				foreach (Type type in assembly.GetTypes())
				{
					ConfigureConsume(configurationModel, autoConfigurationModel, type);
					ConfigurePublish(configurationModel, autoConfigurationModel, type);
					ConfigureSubscriptions(configurationModel, autoConfigurationModel, type);
				}
			}
		}

		void ConfigureSubscriptions(IConfigurationModel configurationModel, IAutoConfigurationModel autoConfigurationModel, Type handlerType)
		{
			foreach (ISubscriptionConvention convention in autoConfigurationModel.SubscriptionConventions)
			{
				if (convention.ShouldRegister(handlerType))
				{
					object handler = Activator.CreateInstance(handlerType);
					Type messageType = convention.GetMessageType(handler);
					MethodInfo openGetMessageHandlerMethodInfo =
						typeof (ISubscriptionConvention).GetMethod("GetMessageHandler", BindingFlags.Instance | BindingFlags.Public);
					MethodInfo closedGetMessageHandlerMethodInfo =
						openGetMessageHandlerMethodInfo.MakeGenericMethod(new[] {messageType});
					object messageHandler = closedGetMessageHandlerMethodInfo.Invoke(convention, new[] {handler});
					configurationModel.AutoSubscriptions.Add(new AutoSubscription(messageType, messageHandler));
				}
			}
		}

		void ConfigurePublish(IConfigurationModel configurationModel, IAutoConfigurationModel autoConfigurationModel,
		                      Type type)
		{
			foreach (IPublishConfigurationConvention convention in autoConfigurationModel.PublishConfigurationConventions)
			{
				if (convention.ShouldRegister(type))
				{
					PublishInfo publishInfo = GetPublishInfo(type, convention);
					configurationModel.PublishRouteConfiguration.AddPolicy<MappingRouteInfoLookupStrategy<IPublishInfo>>(type,
					                                                                                                     publishInfo);
				}
			}
		}

		static PublishInfo GetPublishInfo(Type type, IPublishConfigurationConvention convention)
		{
			var publishInfo = new PublishInfo
			                  	{
			                  		ExchangeName = convention.GetExchangeName(type),
			                  		IsAutoDelete = convention.IsAutoDeleteExchange(type),
			                  		IsDurable = convention.IsDurableExchange(type),
			                  		IsPersistent = convention.IsPersistent(type),
			                  		ExchangeType = convention.GetExchangeType(type),
			                  		DefaultRoutingKey = convention.GetDefaultRouteKey(type),
			                  		IsSigned = convention.IsSigned(type)
			                  	};

			ISerializationStrategy serializationStrategy = convention.GetSerializationStrategy(type);
			if (serializationStrategy != null)
			{
				publishInfo.SerializationStrategy = serializationStrategy;
			}
			return publishInfo;
		}

		static void ConfigureConsume(IConfigurationModel configurationModel, IAutoConfigurationModel autoConfigurationModel,
		                             Type type)
		{
			foreach (IConsumeConfigurationConvention convention in autoConfigurationModel.ConsumeConfigurationConventions)
			{
				if (convention.ShouldRegister(type))
				{
					ConsumeInfo consumeInfo = GetConsumeInfo(type, convention);

					configurationModel.ConsumeRouteConfiguration
						.AddPolicy<MappingRouteInfoLookupStrategy<IConsumeInfo>>(type, consumeInfo);
				}
			}
		}

		static ConsumeInfo GetConsumeInfo(Type type, IConsumeConfigurationConvention convention)
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
			                  		QualityOfService = convention.GetQualityOfService(type)
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
			return consumeInfo;
		}
	}
}