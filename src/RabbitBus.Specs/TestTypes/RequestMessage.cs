using System;

namespace RabbitBus.Specs.TestTypes
{
	[Serializable]
	class RequestMessage
	{
		public RequestMessage(string text)
		{
			Text = text;
		}

		public string Text { get; set; }
	}
}