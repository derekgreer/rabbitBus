using System;
using System.Threading;
using Machine.Specifications;
using RabbitBus.Serialization.Json;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

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

		Cleanup after = () =>
			{
				_client.Close();
				_server.Close();
			};

		Because of = () =>
		             new Action(() => _client.Publish<RequestMessage, ReplyMessage>(new RequestMessage("text"), mc =>
		             	{
		             		_mc = mc;
		             		mc.AcceptMessage();
		             	})).BlockUntil(() => _mc != null)();

		It should_receive_the_expected_correlation_id = () => _mc.CorrelationId.ShouldEqual(_expectedCorrelationId);

		It should_receive_the_response_message = () => _mc.Message.Text.ShouldEqual("reply");
	}

	[Integration]
	[Subject("Remote Procedure Calls")]
	public class when_publishing_remote_procedure_call_requests_to_a_persistent_durable_exchange
	{
		const string SpecId = "2A2550E6-2159-473A-8B41-65D73F09DB78";
		static IMessageContext<ReplyMessage> _mc;
		static Bus _client;
		static Bus _server;
		static string _expectedCorrelationId = "unset";

		Establish context = () =>
		{
			_client = new BusBuilder()
				.Configure(ctx => ctx
														.WithLogger(new ConsoleLogger())
														.Publish<RequestMessage>()
														.WithExchange(SpecId, cfg => cfg.Fanout().Not.AutoDelete().Durable())
														.WithSerializationStrategy(new JsonSerializationStrategy())
														.OnReplyError(x => { }))
				.Build();
			_client.Connect();

			_server = new BusBuilder()
				.Configure(ctx => ctx
														.WithLogger(new ConsoleLogger())
														.Consume<RequestMessage>()
														.WithSerializationStrategy(new JsonSerializationStrategy())
														.WithExchange(SpecId, cfg => cfg.Fanout().Not.AutoDelete().Durable())
														.WithQueue(SpecId, cfg => cfg.Not.AutoDelete().Durable()))
				.Build();
			_server.Connect();

			_server.Subscribe<RequestMessage>(ctx =>
			{
				_expectedCorrelationId = ctx.CorrelationId;
				ctx.Reply(new ReplyMessage("reply"));
				ctx.AcceptMessage();
			});
		};

		Cleanup after = () =>
		{
			_client.Close();
			_server.Close();
			new RabbitExchange("localhost", SpecId, ExchangeType.Fanout, true, false).Delete(false).Close();
		};

		Because of = () =>
								 new Action(() => _client.Publish<RequestMessage, ReplyMessage>(new RequestMessage("text"), mc =>
								 {
									 _mc = mc;
									 mc.AcceptMessage();
								 })).BlockUntil(() => _mc != null)();

		It should_receive_the_expected_correlation_id = () => _mc.CorrelationId.ShouldEqual(_expectedCorrelationId);

		It should_receive_the_response_message = () => _mc.Message.Text.ShouldEqual("reply");
	}

	[Integration]
	[Subject("Remote Procedure Calls")]
	public class when_making_a_remote_procedure_call_with_timeout
	{
		const string SpecId = "17F677FC-7C0B-4A83-B27E-5E1DE46279FB";
		static Bus _client;
		static Bus _server;
		static IMessageContext<ReplyMessage> _mc;
		static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);
		static string _message = "timeout";

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
						Thread.Sleep(10000);
						ctx.Reply(new ReplyMessage("reply"));
						ctx.AcceptMessage();
					});
			};

		Cleanup after = () =>
			{
				_client.Close();
				_server.Close();
			};

		Because of = () =>
		             new Action(() => _client.Publish<RequestMessage, ReplyMessage>(new RequestMessage("text"), mc =>
		             	{
		             		_mc = mc;
		             		_message = mc.Message.Text;
		             		mc.AcceptMessage();
		             	}, Timeout)).BlockUntil(() => _mc != null)();

		It should_timeout_after_the_specified_time = () => _message.ShouldNotEqual("reply");
	}
}