using System;
using Machine.Specifications;
using RabbitBus.Serialization.Json;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Remote Procedure Calls")]
	public class when_a_remote_procedure_call_is_responded_to
	{
		const string SpecId = "58F7CA14-FD21-451B-99BC-C898A72B116D";
		static IMessageContext<ReplyMessage> _mc;
		static Bus _client;
		static Bus _server;
		static string _expectedCorrelationId = "unset";

		Cleanup after = () =>
			{
				_client.Close();
				_server.Close();
			};

		Establish context = () =>
			{
				_client = new BusBuilder()
					.Configure(ctx => ctx
					                  	.WithLogger(new ConsoleLogger())
					                  	.Publish<RequestMessage>()
					                  	.WithExchange(SpecId)
															.WithSerializationStrategy(new JsonSerializationStrategy())
					                  	.OnReplyError(x => { }))
					.Build();
				_client.Connect();

				_server = new BusBuilder()
					.Configure(ctx => ctx
					                  	.WithLogger(new ConsoleLogger())
					                  	.Consume<RequestMessage>()
															.WithSerializationStrategy(new JsonSerializationStrategy())
					                  	.WithExchange(SpecId)
					                  	.WithQueue(SpecId))
					.Build();
				_server.Connect();

				_server.Subscribe<RequestMessage>(ctx =>
					{
						_expectedCorrelationId = ctx.CorrelationId;
						ctx.Reply(new ReplyMessage("reply"));
						ctx.AcceptMessage();
					});
			};

		Because of =
			() =>
			new Action(() => _client.Publish<RequestMessage, ReplyMessage>(new RequestMessage("text"), mc =>
				{
					_mc = mc;
					mc.AcceptMessage();
				})).
				BlockUntil(() => _mc != null)();

		It should_receive_the_expected_correlation_id = () => _mc.CorrelationId.ShouldEqual(_expectedCorrelationId);

		It should_receive_the_response_message = () => _mc.Message.Text.ShouldEqual("reply");
	}
}