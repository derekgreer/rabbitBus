using System;
using System.Runtime.Serialization;
using ExpectedObjects;
using Machine.Specifications;
using Moq;
using RabbitBus.Logging;
using RabbitBus.Serialization.Json;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;
using It = Machine.Specifications.It;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Json Serialization")]
	public class when_subscribing_to_a_message_published_with_json_serialization
	{
		const string SpecId = "C6866649-999E-446B-81FA-51982190E729";
		static JsonSerializationStrategy _serializationStrategy;
		static RabbitExchange _exchange;
		static ExpectedObject _expectedMessage;
		static TestMessage _actualMessage;

		Establish context = () =>
			{
				_serializationStrategy = new JsonSerializationStrategy();
				_exchange = new RabbitExchange("localhost", SpecId, ExchangeType.Direct);
				_expectedMessage = new TestMessage("test").ToExpectedObject();

				Bus bus = new BusBuilder().Configure(ctx => ctx
				                                            	.WithDefaultSerializationStrategy(_serializationStrategy)
				                                            	.Consume<TestMessage>().WithExchange(SpecId).WithQueue(SpecId)).Build();
				bus.Connect();
				bus.Subscribe<TestMessage>(messageContext =>
					{
						_actualMessage = messageContext.Message;
						messageContext.AcceptMessage();
						bus.Close();
					});
			};

		Cleanup after = () => _exchange.Delete().Close();

		Because of =
			() =>
			new Action(() => _exchange.Publish(new TestMessage("test"), string.Empty, _serializationStrategy)).BlockUntil(
				() => _actualMessage != null)();

		It should_deserialize_correctly = () => _expectedMessage.ShouldEqual(_actualMessage);
	}


	[Integration]
	[Subject("Json Serialization")]
	public class when_publishing_a_message_with_json_serialization
	{
		const string SpecId = "331791CA-ACAB-4BB1-9C22-CDB7711E8FA6";
		static RabbitQueue _rabbitQueue;
		static TestMessage _actualMessage;
		static Bus _bus;
		static ExpectedObject _expectedMessage;
		static JsonSerializationStrategy _serializationStrategy;

		Establish context = () =>
			{
				_serializationStrategy = new JsonSerializationStrategy();
				_expectedMessage = new TestMessage("test").ToExpectedObject();

				_bus = new BusBuilder().Configure(ctx => ctx.WithLogger(new ConsoleLogger())
				                                         	.WithDefaultSerializationStrategy(_serializationStrategy)
				                                         	.Publish<TestMessage>().WithExchange(SpecId)).Build();
				_bus.Connect();
				_rabbitQueue = new RabbitQueue("localhost", SpecId, ExchangeType.Direct, SpecId);
			};

		Cleanup after = () =>
			{
				_rabbitQueue.Close();
				_bus.Close();
			};

		Because of = () => _bus.Publish(new TestMessage("test"));

		It should_serialize_correctly =
			() => _expectedMessage.ShouldEqual(_rabbitQueue.GetMessage<TestMessage>(_serializationStrategy));
	}

	[Integration]
	[Subject("Json Serialization")]
	public class when_a_deserialization_error_occurs
	{
		const string ExpectedErrorMessage = "An error occurred attempting to deserialize the provided data:";
		static JsonSerializationStrategy _serializationStrategy;
		static string _actualErrorMessage;
		static Exception _exception;

		Establish context = () =>
			{
				var loggingProviderSpy = new Mock<ILogger>();
				loggingProviderSpy.Setup(x => x.Write(Moq.It.IsAny<LogEntry>()))
					.Callback<LogEntry>(e => _actualErrorMessage = e.Message);
				_serializationStrategy = new JsonSerializationStrategy();
				Logger.UseLogger(loggingProviderSpy.Object);
			};

		Because of = () => _exception = Catch.Exception(() => _serializationStrategy.Deserialize<TestMessage>(null));

		It should_log_an_error_message = () => _actualErrorMessage.ShouldStartWith(ExpectedErrorMessage);

		It should_throw_a_deserialization_exception = () => _exception.ShouldBeOfType(typeof (SerializationException));
	}

	[Integration]
	[Subject("Json Serialization")]
	public class when_a_serialization_error_occurs
	{
		const string ExpectedErrorMessage = "An error occurred attempting to serialize the provided message:";
		static JsonSerializationStrategy _serializationStrategy;
		static string _actualErrorMessage;
		static Exception _exception;

		Establish context = () =>
			{
				var loggingProviderSpy = new Mock<ILogger>();
				loggingProviderSpy.Setup(x => x.Write(Moq.It.IsAny<LogEntry>()))
					.Callback<LogEntry>(e => _actualErrorMessage = e.Message);
				_serializationStrategy = new JsonSerializationStrategy();
				Logger.UseLogger(loggingProviderSpy.Object);
			};

		Because of = () => _exception = Catch.Exception(() => _serializationStrategy.Serialize(new Unserializable()));

		It should_log_an_error_message = () => _actualErrorMessage.ShouldStartWith(ExpectedErrorMessage);

		It should_throw_a_deserialization_exception = () => _exception.ShouldBeOfType(typeof (SerializationException));
	}

	class Unserializable
	{
		public string Property
		{
			get { throw new Exception("Boom!"); }
		}
	}
}