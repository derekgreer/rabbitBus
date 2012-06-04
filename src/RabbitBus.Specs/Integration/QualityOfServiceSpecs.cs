using System;
using System.Linq;
using Machine.Specifications;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Quality of service")]
	public class when_setting_the_quality_of_service
	{
		const string SpecId = "7759CCD4-EA56-4EBF-8489-FDA0BC2B4796";
		static int _messagesReceived;
		static Bus _bus1;
		static Bus _bus2;
		static RabbitExchange _exchange;
		static int _messagesReceived2;
		static int _messagesReceived1;

		Establish context = () =>
			{
				_bus1 = new BusBuilder()
					.Configure(ctx => ctx.Consume<TestMessage>()
					                  	.WithExchange(SpecId)
					                  	.WithQueue(SpecId, cfg => cfg.UnacknowledgeLimit(1)))
					.Build();
				_bus1.Connect();

				_bus2 =
					new BusBuilder()
						.Configure(ctx => ctx.Consume<TestMessage>()
						                  	.WithExchange(SpecId)
						                  	.WithQueue(SpecId, cfg => cfg.UnacknowledgeLimit(1)))
						.Build();
				_bus2.Connect();

				_exchange = new RabbitExchange(SpecId);

				_bus1.Subscribe<TestMessage>(messageContext => { _messagesReceived1++; });

				_bus2.Subscribe<TestMessage>(messageContext =>
					{
						_messagesReceived2++;
						messageContext.AcceptMessage();
					});
			};

		Cleanup after = () =>
			{
				_bus1.Close();
				_bus2.Close();
				_exchange.Close();
			};

		Because of =
			() => new Action(() => Enumerable.Range(0, 4).ToList().ForEach(i => _exchange.Publish(new TestMessage("."))))
			      	.BlockUntil(() => (_messagesReceived1 + _messagesReceived2) == 4)();

		It should_observe_the_quality_of_service_settings = () => _messagesReceived1.ShouldEqual(1);
	}
}