using System;
using System.Threading;

namespace RabbitBus
{
	public class TimeProvider
	{
		static ITimeProvider _current = new DefaultTimeProvider();

		public static ITimeProvider Current { get { return _current; } }
		
		public static void SetCurrent(ITimeProvider timeProvider)
		{
			_current = timeProvider;
		}

		class DefaultTimeProvider : ITimeProvider
		{
			public void Sleep(TimeSpan timeSpan)
			{
				Thread.Sleep(timeSpan);
			}
		}
	}
}