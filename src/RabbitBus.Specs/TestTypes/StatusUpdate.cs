using System;

namespace RabbitBus.Specs.TestTypes
{
	[Serializable]
	public class StatusUpdate
	{
		public StatusUpdate()
		{

		}

		public StatusUpdate(string text)
		{
			Text = text;
		}

		public string Text { get; set; }
	}
}