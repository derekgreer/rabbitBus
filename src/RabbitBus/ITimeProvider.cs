using System;

namespace RabbitBus
{
	public interface ITimeProvider
	{
		void Sleep(TimeSpan timeSpan);
	}
}