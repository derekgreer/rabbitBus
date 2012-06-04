using System;
using Machine.Specifications;
using Moq;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;
using It = Machine.Specifications.It;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Message Time")]
	public class when_a_message_is_redelivered
	{
		const string SpecId = "6CCCCB32-A3E5-4A74-B4B0-7FC4DEA95172";
		static RabbitExchange _exchange;
		static IMessageContext<TestMessage> _messageContext = new Mock<IMessageContext<TestMessage>>().Object;
		static bool _redelivered;

		Establish context = () =>
			{
				_exchange = new RabbitExchange("localhost", SpecId, ExchangeType.Direct);

				var bus = new BusBuilder().Configure(ctx => ctx.Consume<TestMessage>().WithExchange(SpecId).WithQueue(SpecId)).Build();
				bus.Connect();
				var requeued = false;
				bus.Subscribe<TestMessage>(context =>
					{
						context.RejectMessage(true);

						if (requeued)
						{
							_redelivered = true;
							_messageContext = context;
							bus.Close();
						}

						requeued = true;
					});
			};

		Cleanup after = () => _exchange.Delete().Close();

		Because of = () => new Action(() => _exchange.Publish(new TestMessage("test"))).BlockUntil(() => _redelivered)();

		It should_be_marked_redelivered = () => _messageContext.Redelivered.ShouldBeTrue();
	}
}