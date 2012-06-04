using System;

namespace RabbitBus.Specs.TestTypes
{
	[Serializable]
	class ReplyMessage
	{
		public ReplyMessage(string text)
		{
			Text = text;
		}

		public string Text { get; set; }
	}
}