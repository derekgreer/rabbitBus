using System;

namespace RabbitBus.Specs.TestTypes
{
	[Serializable]
	public class TestMessageWithProtectedTypes
	{
		public TestMessageWithProtectedTypes(string expectedMessage)
		{
			Text = expectedMessage;
		}

		protected string Text { get; set; }

		public string GetText()
		{
			return Text;
		}
	}
}