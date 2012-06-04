using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Machine.Specifications;
using RabbitBus.Configuration;
using RabbitBus.Configuration.Internal;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Headers Exchange")]
	public class when_configuring_a_message_with_a_headers_exchange
	{
		static string _exchangeType;
		static IPublishConfigurationContext _publishConfigurationContext;

		Establish context =
			() =>
			new BusBuilder().Configure(
				ctx => _publishConfigurationContext = ctx.Publish<TestMessage>().WithExchange("any", cfg => cfg.Headers())).Build();

		It should_set_exchange_type_to_headers =
			() => ((IPublishInfoSource) _publishConfigurationContext).PublishInfo.ExchangeType.ShouldEqual(ExchangeType.Headers);
	}

	[Integration]
	[Subject("Headers Exchange")]
	public class when_subscribed_to_recieve_non_matching_messages_from_a_header_exchange
	{
		const string SpecId = "EE34BDA4-1175-49D3-9B65-B624F7D01715";
		static RabbitExchange _exchange;
		static Bus _bus;
		static TestMessage _actualMessage;
		static readonly TestMessage _default = new TestMessage("error");
		static IDictionary _headers;

		Establish context = () =>
			{
				_headers = new Dictionary<string, object>();
				_headers.Add("header-key", Encoding.UTF8.GetBytes("header key value"));

				_exchange = new RabbitExchange(SpecId, ExchangeType.Headers);
				_bus =
					new BusBuilder().Configure(
						ctx => ctx.Consume<TestMessage>().WithExchange(SpecId, cfg => cfg.Headers()).WithQueue(SpecId)).Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(messageContext => { _actualMessage = messageContext.Message; }, _headers);
			};

		Cleanup after = () => _bus.Close();

		Because of =
			() =>
			new Action(() => _exchange.Publish(new TestMessage("test"), new Dictionary<string, object>())).BlockUntil(
				() => _actualMessage != null)();

		It should_not_receive_the_message = () => _actualMessage.ShouldBeNull();
	}

	[Integration]
	[Subject("Headers Exchange")]
	public class when_subscribed_to_recieve_matching_messages_from_a_header_exchange
	{
		const string SpecId = "0B7720E6-CE0F-4BB3-9ED3-82562A203004";
		static RabbitExchange _exchange;
		static Bus _bus;
		static TestMessage _actualMessage;
		static readonly TestMessage _default = new TestMessage("error");
		static IDictionary _headers;

		Establish context = () =>
			{
				_headers = new Dictionary<string, object>();
				_headers.Add("header-key", Encoding.UTF8.GetBytes("header key value"));

				_exchange = new RabbitExchange(SpecId, ExchangeType.Headers);
				_bus = new BusBuilder().Configure(
					ctx => ctx.Consume<TestMessage>().WithExchange(SpecId, cfg => cfg.Headers()).WithQueue(SpecId)).Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(messageContext => { _actualMessage = messageContext.Message; }, _headers);
			};

		Cleanup after = () => _bus.Close();

		Because of =
			() =>
			new Action(() => _exchange.Publish(new TestMessage("test"), _headers)).BlockUntil(() => _actualMessage != null)();

		It should_receive_the_message = () => _actualMessage.ProvideDefault(() => _default).Text.ShouldEqual("test");
	}

	[Integration]
	[Subject("Headers exchanges")]
	public class when_publishing_messages_to_a_headers_exchange2
	{
		const string SpecId = "DDAB5940-9366-41D3-A7C7-28AC6A84A383";
		static RabbitQueue _queue;
		static IBasicProperties _messageProperties;
		static Bus _bus;

		Cleanup after = () =>
		{
			_bus.Close();
			_queue.Delete().Close();
		};

		Establish context = () =>
		{
			var headers = new Dictionary<string, object>();
			headers.Add("header-key", Encoding.UTF8.GetBytes("header key value"));


			_bus = new BusBuilder().Configure(ctx => ctx
														.Publish<TestMessage>()
														.WithExchange(SpecId, cfg => cfg.Headers()))
														.Build();
			_bus.Connect();
			_queue = new RabbitQueue("localhost", SpecId, ExchangeType.Headers, SpecId, false, true, false, true, "", headers);
			_bus.Publish(new TestMessage("test"), headers);
		};

		Because of = () => new Action(() => _messageProperties = _queue.GetMessageProperties<TestMessage>()).ExecuteUntil(
				() => _messageProperties != null)();

		It should_publish_the_message_with_headers = () => _messageProperties.Headers.Keys.ShouldContain("header-key");
	}

	[Integration]
	[Subject("Headers exchanges")]
	public class when_publishing_messages_to_a_headers_exchange
	{
		const string SpecId = "DDAB5940-9366-41D3-A7C7-28AC6A84A383";
		static RabbitQueue _queue;
		static IBasicProperties _messageProperties;
		static Bus _bus;

		Establish context = () =>
			{
				var headers = new Dictionary<string, object>();
				headers.Add("header-key", Encoding.UTF8.GetBytes("header key value"));


				_bus = new BusBuilder().Configure(ctx => ctx
				                                         	.Publish<TestMessage>()
				                                         	.WithExchange(SpecId, cfg => cfg.Headers()))
					.Build();
				_bus.Connect();
				_queue = new RabbitQueue("localhost", SpecId, ExchangeType.Headers, SpecId, false, true, false, true, "", headers);
				_bus.Publish(new TestMessage("test"), headers);
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_queue.Delete().Close();
			};

		Because of = () => new Action(() => _messageProperties = _queue.GetMessageProperties<TestMessage>()).ExecuteUntil(
			() => _messageProperties != null)();

		It should_publish_the_message_with_headers = () => _messageProperties.Headers.Keys.ShouldContain("header-key");
	}

	[Integration]
	[Subject("Headers exchanges")]
	public class when_publishing_messages_with_default_headers
	{
		const string SpecId = "12BA6C54-242E-4EF7-B61A-7BEA232993DC";
		static RabbitQueue _queue;
		static IBasicProperties _messageProperties;
		static Bus _bus;

		Establish context = () =>
			{
				var headers = new Dictionary<string, object>();
				headers.Add("header-key", Encoding.UTF8.GetBytes("header key value"));


				_bus = new BusBuilder().Configure(ctx => ctx
				                                         	.Publish<TestMessage>()
				                                         	.WithExchange(SpecId)
				                                         	.WithDefaultHeaders(headers))
					.Build();
				_bus.Connect();
				_queue = new RabbitQueue("localhost", SpecId, ExchangeType.Direct, SpecId, false, true, false, true, "", headers);
				_bus.Publish(new TestMessage("test"));
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_queue.Delete().Close();
			};

		Because of = () => new Action(() => _messageProperties = _queue.GetMessageProperties<TestMessage>())
		                   	.ExecuteUntil(() => _messageProperties != null)();

		It should_publish_the_message_with_headers = () => _messageProperties.Headers.Keys.ShouldContain("header-key");
	}

	[Integration]
	[Subject("Headers exchanges")]
	public class when_publishing_rpc_messages_with_default_headers
	{
		const string SpecId = "15192820-7236-444E-9EA8-5638F2CC3AC8";
		static RabbitQueue _queue;
		static IBasicProperties _messageProperties;
		static Bus _bus;

		Establish context = () =>
		{
			var headers = new Dictionary<string, object>();
			headers.Add("header-key", Encoding.UTF8.GetBytes("header key value"));


			_bus = new BusBuilder().Configure(ctx => ctx
																								.Publish<RequestMessage>()
																								.WithExchange(SpecId)
																								.WithDefaultHeaders(headers))
				.Build();
			_bus.Connect();
			_queue = new RabbitQueue(SpecId, SpecId);
			_bus.Publish<RequestMessage, ReplyMessage>(new RequestMessage("test"), mc => { /* This will never get called because there's no server */ });
		};

		Cleanup after = () =>
		{
			_bus.Close();
			_queue.Delete().Close();
		};

		Because of = () => new Action(() => _messageProperties = _queue.GetMessageProperties<RequestMessage>())
												.ExecuteUntil(() => _messageProperties != null)();

		It should_publish_the_message_with_headers = () => _messageProperties.Headers.Keys.ShouldContain("header-key");
	}
}