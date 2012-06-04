using System;
using System.Collections;
using System.Collections.Generic;
using Machine.Specifications;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Unsubscribing")]
	public class when_unsubscribing_from_a_queue
	{
		const string SpecId = "A11C2DEB-CB28-4ED9-9976-14083031A07F";
		const string ExchangeName = "unsubscribe-test";
		const string QueueName = "unsubscribe-test";
		const string RabbitMqHost = "localhost";
		static RabbitExchange _exchange;
		static Bus _bus;
		static TestMessage _actualMessage = new TestMessage("original");


		Establish context = () =>
			{
				_exchange = new RabbitExchange(RabbitMqHost, ExchangeName, ExchangeType.Direct);

				_bus = new BusBuilder().Configure(ctx => ctx.Consume<TestMessage>().WithExchange(SpecId)).Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(m =>
					{
						_actualMessage = m.Message;
						Console.WriteLine("message received:" + m.Message.Text +
						                  " " + m.Id);
						m.AcceptMessage();
					});
				_bus.Unsubscribe<TestMessage>();
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_exchange.Delete().Close();
			};

		Because of = () => new Action(() => _exchange.Publish(new TestMessage("test"))).BlockUntil(() => _actualMessage.Text != "original")();


		It should_no_longer_receive_published_messages = () => _actualMessage.Text.ShouldEqual("original");
	}

	[Integration]
	[Subject("Unsubscribing")]
	public class when_unsubscribing_from_a_queue_with_routing_key
	{
		const string SpecId = "32426D0B-FC36-40EF-8289-3C268EB58B66";
		static RabbitExchange _exchange;
		static Bus _bus;
		static TestMessage _actualMessage = new TestMessage("original");


		Establish context = () =>
			{
				_exchange = new RabbitExchange("localhost", SpecId, ExchangeType.Direct);
				_bus = new BusBuilder().Configure(ctx => ctx.Consume<TestMessage>().WithExchange(SpecId, cfg => cfg.Headers()).WithDefaultRoutingKey("key").WithQueue(SpecId)).Build();
				_bus.Connect();

				_bus.Subscribe<TestMessage>(m =>
					{
						_actualMessage = m.Message;
						Console.WriteLine("message received:" + m.Message.Text +
						                  " " + m.Id);
						m.AcceptMessage();
					});
				_bus.Unsubscribe<TestMessage>("key");
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_exchange.Delete().Close();
			};

		Because of = () => new Action(() => _exchange.Publish(new TestMessage("test"))).BlockUntil(() => _actualMessage.Text != "original")();


		It should_no_longer_receive_published_messages = () => _actualMessage.Text.ShouldEqual("original");
	}

	[Integration]
	[Subject("Unsubscribing")]
	public class when_unsubscribing_from_a_queue_with_headers
	{
		const string SpecId = "0F600913-8577-4F04-A5DD-342F5659E400";
		static RabbitExchange _exchange;
		static Bus _bus;
		static TestMessage _actualMessage = new TestMessage("original");
		static IDictionary _headers;


		Establish context = () =>
			{
				_exchange = new RabbitExchange("localhost", SpecId, ExchangeType.Direct);
				_bus = new BusBuilder().Configure(ctx => ctx.Consume<TestMessage>().WithExchange(SpecId).WithQueue(SpecId)).Build();
				_bus.Connect();

				_headers = new Dictionary<object, string>();

				_bus.Subscribe<TestMessage>(m =>
					{
						_actualMessage = m.Message;
						Console.WriteLine("message received:" + m.Message.Text +
						                  " " + m.Id);
						m.AcceptMessage();
					}, _headers);

				_bus.Unsubscribe<TestMessage>(_headers);
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_exchange.Delete().Close();
			};

		Because of = () => new Action(() => _exchange.Publish(new TestMessage("test"), _headers)).BlockUntil(() => _actualMessage.Text != "original")();


		It should_no_longer_receive_published_messages = () => _actualMessage.Text.ShouldEqual("original");
	}
}