using System;
using System.Threading;

namespace RabbitBus.Specs.Infrastructure
{
	public static class ActionExtensions
	{
		const int Timeout = 15;

		public static Action Then(this Action action, Action afterAction)
		{
			return () =>
				{
					action();
					afterAction();
				};
		}

		public static Action Background(this Action action)
		{
			return () =>
			{
				var thread = new Thread(() => action());
				thread.Start();
			};
		}

		public static Action BlockUntil(this Action action, Func<bool> predicate)
		{
			return () =>
				{
					DateTime start = DateTime.Now;
					action();
					while (!predicate())
					{
						if (DateTime.Now.Subtract(start).Seconds >= Timeout)
						{
							Console.WriteLine("Timeout");
							break;
						}
					
						Thread.Sleep(1000);
					}
				};
			
		}

		public static void Execute(this Action action)
		{
			action();
		}

		public static Action ExecuteUntil(this Action action, Func<bool> predicate)
		{
			return () =>
				{
					DateTime start = DateTime.Now;

					while (!predicate())
					{
						action();

						if (DateTime.Now.Subtract(start).Seconds >= Timeout)
							break;

						Thread.Sleep(1000);
					}
				};
		}

		public static void RepeatUntilSuccessfulOrTimeout(this Action action)
		{
			Exception exception = null;
			DateTime start = DateTime.Now;

			do
			{
				try
				{
					exception = null;
					action();
				}
				catch (Exception e)
				{
					Console.WriteLine("Observation failed. Retrying observation for " +
					                  (Timeout - DateTime.Now.Subtract(start).Seconds) + " seconds.");
					exception = e;
					Thread.Sleep(1000);
				}

				if (DateTime.Now.Subtract(start).Seconds >= Timeout)
				{
					break;
				}
			} while (
				exception != null);

			if (exception != null) throw exception;
		}
	}
}