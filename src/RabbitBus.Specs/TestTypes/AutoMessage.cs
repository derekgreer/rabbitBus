using System;

namespace RabbitBus.Specs.TestTypes
{
	[Serializable]
	public class AutoMessage
	{
		public AutoMessage()
		{
			
		}

		public AutoMessage(string text)
		{
			Text = text;
		}

		public string Text { get; set; }
	}
}