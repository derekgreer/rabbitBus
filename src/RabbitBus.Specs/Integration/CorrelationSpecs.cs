using System;
using Machine.Specifications;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Correlation")]
	public class when_subscribing_to_a_message_published_with_a_correlation_id_set
	{
		const string SpecId = "F134515B-ED8A-4B6A-8632-A4839EB5534F";
		static Bus _publisher;
		static Bus _subscriber;
		static string _actualCorrelationId;

		Establish context = () =>
			{
				_publisher = new BusBuilder().Configure(ctx => ctx.Publish<TestMessage>().WithExchange(SpecId)).Build();
				_publisher.Connect();
				_subscriber = new BusBuilder().Configure(ctx => ctx.WithLogger(new ConsoleLogger())
				                                                	.Consume<TestMessage>()
				                                                	.WithExchange(SpecId)
				                                                	.WithQueue(SpecId)).Build();
				_subscriber.Connect();
				_subscriber.Subscribe<TestMessage>(context => { _actualCorrelationId = context.CorrelationId; });
			};

		Cleanup after = () =>
			{
				_publisher.Close();
				_subscriber.Close();
			};

		Because of =
			() => new Action(() => _publisher.Publish(new TestMessage("test"))).BlockUntil(() => _actualCorrelationId != null)();

		It should_receive_the_message_with_a_correlation_id = () => _actualCorrelationId.ShouldNotBeNull();
	}
}