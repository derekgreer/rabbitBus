using System;

namespace RabbitBus.Specs.Common
{
	[Serializable]
	public class TestMessage
	{
		public TestMessage(string expectedMessage)
		{
			Text = expectedMessage;
		}

		public string Text { get; set; }
	}

	[Serializable]
	public class TestMessage2 : TestMessage
	{
		public TestMessage2(string expectedMessage) : base(expectedMessage)
		{
		}
	}
}