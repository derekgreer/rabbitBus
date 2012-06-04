using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using RabbitBus.Configuration;
using RabbitBus.Specs.Common;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Publishing")]
	public class when_publishing_a_message_with_headers
	{
		const string SpecId = "E28174CE-D999-487C-A5B9-85DB53851262";
		const string ExchangeName = SpecId;
		const string QueueName = SpecId;
		const string ExpectedMessage = SpecId;
		static Bus _bus;
		static RabbitQueue _rabbitQueue;
		static byte[] _expectedValue;

		Establish context = () =>
			{
				IDictionary headers = new Dictionary<string, object>();
				headers.Add("key", "value");

				_bus =
					new BusBuilder()
						.Configure(
							ctx => ctx
							       	.WithLogger(new ConsoleLogger())
							       	.Publish<TestMessage>().WithExchange(SpecId, cfg => cfg.Fanout())
							       	.WithHeaders(headers))
						.Build();
				_bus.Connect();
				_rabbitQueue = new RabbitQueue("localhost", SpecId, ExchangeType.Fanout, SpecId);
				_expectedValue = new BinarySerializationStrategy().Serialize("value");
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_rabbitQueue.Delete().Close();
			};

		Because of = () => _bus.Publish(new TestMessage(ExpectedMessage));

		It should_publish_the_message_with_the_specified_headers =
			() =>
			_rabbitQueue.GetMessageProperties<TestMessage>().Headers.ToDictionary<object, object>().ToList()
				.FirstOrDefault(x => x.Key.Equals("key")).Value.ShouldEqual(_expectedValue);
	}
}