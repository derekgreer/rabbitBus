using Machine.Specifications;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Publishing")]
	public class when_publishing_a_message
	{
		const string SpecId = "E28174CE-D999-487C-A5B9-85DB53851262";
		const string ExchangeName = SpecId;
		const string QueueName = SpecId;
		const string ExpectedMessage = SpecId;
		static Bus _bus;
		static RabbitQueue _rabbitQueue;

		Establish context = () =>
			{
				_bus = new BusBuilder().Configure(ctx => ctx.WithLogger(new ConsoleLogger()).Publish<TestMessage>().WithExchange(SpecId, cfg => cfg.Fanout())).Build();
				_bus.Connect();
				_rabbitQueue = new RabbitQueue("localhost", SpecId, ExchangeType.Fanout, SpecId);
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_rabbitQueue.Delete().Close();
			};

		Because of = () => _bus.Publish(new TestMessage(ExpectedMessage));

		It should_publish_the_message = () => _rabbitQueue.GetMessage<TestMessage>().Text.ShouldEqual(ExpectedMessage);
	}
}