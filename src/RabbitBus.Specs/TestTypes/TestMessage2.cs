using System;

namespace RabbitBus.Specs.TestTypes
{
	[Serializable]
	public class TestMessage2 : TestMessage
	{
		public TestMessage2(string expectedMessage) : base(expectedMessage)
		{
		}
	}
}