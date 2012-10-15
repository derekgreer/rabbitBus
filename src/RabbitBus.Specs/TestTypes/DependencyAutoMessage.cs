using System;

namespace RabbitBus.Specs.TestTypes
{
	[Serializable]
	public class DependencyAutoMessage : AutoMessage
	{
		public DependencyAutoMessage(string text)
		{
			Text = text;
		}
	}
}