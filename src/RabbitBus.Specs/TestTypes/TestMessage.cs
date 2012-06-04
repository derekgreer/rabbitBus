using System;

namespace RabbitBus.Specs.TestTypes
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
}