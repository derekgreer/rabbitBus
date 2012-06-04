using System;
using Machine.Specifications;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Security")]
	public class when_subscribing_to_a_message_signed_by_a_user
	{
		const string SpecId = "A91C7D26-DD75-40A6-A191-0E52CD76D8AD";
		static Bus _publisher;
		static RabbitQueue _authorizedQueue;
		static string _userId;
		static Bus _subscriber;

		Establish context = () =>
			{
				_publisher =
					new BusBuilder()
						.Configure(ctx => ctx.WithLogger(new ConsoleLogger())
						                  	.Publish<TestMessage>().WithExchange(SpecId).Signed()).Build();
				_publisher.Connect();
				_subscriber =
					new BusBuilder()
						.Configure(ctx => ctx.WithLogger(new ConsoleLogger())
						                  	.Consume<TestMessage>().WithExchange(SpecId).WithQueue(SpecId)).Build();
				_subscriber.Connect();
				_subscriber.Subscribe<TestMessage>(context => { _userId = context.UserId; });
			};

		Cleanup after = () =>
			{
				_subscriber.Close();
				_publisher.Close();
			};

		Because of = () => new Action(() => _publisher.Publish(new TestMessage("test"))).BlockUntil(() => _userId != null)();

		It should_deliver_the_message_with_the_user_id = () => _userId.ShouldEqual("guest");
	}
}