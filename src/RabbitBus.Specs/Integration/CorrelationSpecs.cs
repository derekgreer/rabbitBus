using System;
using Machine.Specifications;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Correlation")]
	public class when_subscribing_to_a_message_published_with_no_correlation_id_provided
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

		It should_receive_the_message_with_a_correlation_id_set_by_the_library = () => _actualCorrelationId.ShouldNotBeNull();
	}


	[Integration]
	[Subject("Correlation")]
	public class when_subscribing_to_a_message_published_with_a_correlation_id_provided
	{
		private const string SpecId = "F134515B-ED8A-4B6A-8632-A4839EB5534F";
		private const string ExpectedCorrelationId = "506E2847-EBAA-4229-9E19-96438AC10DD8";
		private static Bus _publisher;
		private static Bus _subscriber;
		private static string _actualCorrelationId;

		private Establish context = () =>
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

		private Cleanup after = () =>
			{
				_publisher.Close();
				_subscriber.Close();
			};

		private Because of =
			() =>
			new Action(
				() =>
				_publisher.Publish(new TestMessage("test"),
								   new MessageProperties() {CorrelationId = ExpectedCorrelationId})).BlockUntil(
									   () => _actualCorrelationId != null)();

		private It should_receive_the_message_with_the_correlation_id_provided = () => _actualCorrelationId.ShouldEqual(ExpectedCorrelationId);
	}
}