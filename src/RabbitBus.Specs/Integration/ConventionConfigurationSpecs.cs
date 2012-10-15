using System;
using System.Reflection;
using Machine.Specifications;
using RabbitBus.Configuration;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Convention configuration")]
	public class when_subscribing_by_convention_with_custom_consume_configuration_convention
	{
		const string SpecId = "AD6B7706-6D33-4FDF-9C27-490420893F4A";
		static RabbitExchange _exchange;
		static Bus _bus;

		Establish context = () =>
			{
				_exchange = new RabbitExchange("localhost", SpecId, ExchangeType.Direct, false, true);

				_bus = new BusBuilder()
					.Configure(new AutoConfigurationModelBuilder()
					           	.WithAssembly(Assembly.GetExecutingAssembly())
					           	.WithConsumeConfigurationConvention(new NamedAutoMessageConsumeConfigurationConvention(SpecId, SpecId))
					           	.WithSubscriptionConvention(new MessageHandlerSubscrptionConvention())
					           	.WithDependencyResolver(new TestDependencyResolver())
					           	.Build())
					.Build();
				_bus.Connect();
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_exchange.Close();
			};

		Because of =
			() =>
			new Action(() => _exchange.Publish(new AutoMessage("test"))).BlockUntil(() => AutoMessageHandler.Message != null)();

		It should_receive_the_message_when_published =
			() => AutoMessageHandler.Message.ProvideDefault().Text.ShouldEqual("test");
	}

	[Integration]
	[Subject("Convention configuration")]
	public class when_subscribing_by_convention_without_explicit_consume_registration
	{
		static RabbitExchange _exchange;
		static Bus _bus;

		Establish context = () =>
			{
				_exchange = new RabbitExchange("localhost", "AutoMessage", ExchangeType.Direct, false, true);

				_bus = new BusBuilder()
					.Configure(new AutoConfigurationModelBuilder()
					           	.WithAssembly(Assembly.GetExecutingAssembly())
					           	.WithSubscriptionConvention(new MessageHandlerSubscrptionConvention())
					           	.WithDependencyResolver(new TestDependencyResolver())
					           	.Build())
					.Build();
				_bus.Connect();
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_exchange.Close();
			};

		Because of =
			() =>
			new Action(() => _exchange.Publish(new AutoMessage("test"))).BlockUntil(() => AutoMessageHandler.Message != null)();

		It should_receive_the_message_when_published =
			() => AutoMessageHandler.Message.ProvideDefault().Text.ShouldEqual("test");
	}

	[Integration]
	[Subject("Convention configuration")]
	public class when_subscribing_by_convention_using_the_calling_assembly
	{
		static RabbitExchange _exchange;
		static Bus _bus;

		Establish context = () =>
			{
				_exchange = new RabbitExchange("localhost", "AutoMessage", ExchangeType.Direct, false, true);

				_bus = new BusBuilder()
					.Configure(new AutoConfigurationModelBuilder()
					           	.WithCallingAssembly()
					           	.WithSubscriptionConvention(new MessageHandlerSubscrptionConvention())
					           	.WithDependencyResolver(new TestDependencyResolver())
					           	.Build())
					.Build();
				_bus.Connect();
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_exchange.Close();
			};

		Because of =
			() =>
			new Action(() => _exchange.Publish(new AutoMessage("test"))).BlockUntil(() => AutoMessageHandler.Message != null)();

		It should_receive_the_message_when_published =
			() => AutoMessageHandler.Message.ProvideDefault().Text.ShouldEqual("test");
	}

	[Integration]
	[Subject("Convention configuration")]
	public class when_subscribing_by_convention_with_handler_dependencies
	{
		static RabbitExchange _exchange;
		static Bus _bus;

		Establish context = () =>
			{
				_exchange = new RabbitExchange("localhost", "DependencyAutoMessage", ExchangeType.Direct, false, true);

				_bus = new BusBuilder()
					.Configure(new AutoConfigurationModelBuilder()
					           	.WithCallingAssembly()
					           	.WithSubscriptionConvention(new MessageHandlerSubscrptionConvention())
					           	.WithDependencyResolver(new TestDependencyResolver())
					           	.Build())
					.Build();
				_bus.Connect();
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_exchange.Close();
			};

		Because of =
			() =>
			new Action(() => _exchange.Publish(new DependencyAutoMessage("test"))).BlockUntil(
				() => DependencyAutoMessageHandler.Message != null)();

		It should_receive_the_message_when_published =
			() => DependencyAutoMessageHandler.Message.ProvideDefault().Text.ShouldEqual("test");
	}

	[Integration]
	[Subject("Convention configuration")]
	public class when_subscribing_by_convention_using_the_default_conventions
	{
		static RabbitExchange _exchange;
		static Bus _bus;

		Establish context = () =>
			{
				_exchange = new RabbitExchange("localhost", "AutoMessage", ExchangeType.Direct, false, true);

				_bus = new BusBuilder()
					.Configure(new AutoConfigurationModelBuilder()
					           	.WithCallingAssembly()
					           	.WithDefaultConventions()
					           	.WithDependencyResolver(new TestDependencyResolver())
					           	.Build())
					.Build();
				_bus.Connect();
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_exchange.Close();
			};

		Because of =
			() =>
			new Action(() => _exchange.Publish(new AutoMessage("test"))).BlockUntil(() => AutoMessageHandler.Message != null)();

		It should_receive_the_message_when_published =
			() => AutoMessageHandler.Message.ProvideDefault().Text.ShouldEqual("test");
	}

	[Integration]
	[Subject("Convention configuration")]
	public class when_configuring_publish_by_convention
	{
		const string SpecId = "A67D4746-D162-432D-B7B0-63EC42EDC6F0";
		static Bus _bus;
		static RabbitQueue _queue;

		Establish context = () =>
			{
				_queue = new RabbitQueue("localhost", SpecId, ExchangeType.Direct, SpecId, false, true, false, true);

				var builder = new BusBuilder();

				_bus = new BusBuilder()
					.Configure(new AutoConfigurationModelBuilder()
					           	.WithAssembly(Assembly.GetExecutingAssembly())
					           	.WithPublishConfigurationConvention(new TestPublishConfigurationConvention(SpecId))
					           	.WithDependencyResolver(new TestDependencyResolver())
					           	.Build())
					.Build();
				_bus.Connect();
			};

		Cleanup after = () =>
			{
				_queue.Close();
				_bus.Close();
			};

		Because of = () => _bus.Publish(new AutoMessage("test"));

		It should_be_able_to_publish_messages_matching_convention =
			() => _queue.GetMessage<AutoMessage>().Text.ShouldEqual("test");
	}

	[Integration]
	[Subject("Convention configuration")]
	public class when_configuring_publish_by_convention_using_the_default_conventions
	{
		static Bus _bus;

		Establish context = () =>
			{
				_bus = new BusBuilder()
					.Configure(new AutoConfigurationModelBuilder()
					           	.WithCallingAssembly()
					           	.WithDefaultConventions()
					           	.WithDependencyResolver(new TestDependencyResolver())
					           	.Build())
					.Build();
				_bus.Connect();
			};

		Cleanup after = () => { _bus.Close(); };

		Because of = () => _bus.Publish(new AutoMessage("test"));

		It should_be_able_to_publish_messages_matching_convention =
			() => AutoMessageHandler.Message.ProvideDefault().Text.ShouldEqual("test");
	}
}