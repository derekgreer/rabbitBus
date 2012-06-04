using System;
using System.Threading;

namespace RabbitBus.Specs.Infrastructure
{
	public static class Wait
	{
		const int Timeout = 30;

		public static void Until(Func<bool> predicate)
		{
			DateTime start = DateTime.Now;
			while (!predicate())
			{
				if (DateTime.Now.Subtract(start).Seconds >= Timeout)
				{
					Console.WriteLine("Timeout exceeded.");
					break;
				}

				Thread.Sleep(1000);
			}
		}
	}
}