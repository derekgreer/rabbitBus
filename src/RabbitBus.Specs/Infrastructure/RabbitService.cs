using System;
using System.ServiceProcess;

namespace RabbitBus.Specs.Infrastructure
{
	public class RabbitService
	{
		public void Restart()
		{
			var serviceController = new ServiceController("RabbitMQ", "localhost");
			TimeSpan timeout = TimeSpan.FromMilliseconds(20000);
			Console.WriteLine("Stopping RabbitMQ service ...");
			serviceController.Stop();
			serviceController.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
			Console.WriteLine("RabbitMQ service stopped.");
			Console.WriteLine("Starting RabbitMQ service ...");
			serviceController.Start();
			serviceController.WaitForStatus(ServiceControllerStatus.Running, timeout);
			Console.WriteLine("RabbitMQ service started.");
		}

		public void Start()
		{
			var serviceController = new ServiceController("RabbitMQ", "localhost");
			TimeSpan timeout = TimeSpan.FromMilliseconds(20000);
			Console.WriteLine("Starting RabbitMQ service ...");
			serviceController.Start();
			serviceController.WaitForStatus(ServiceControllerStatus.Running, timeout);
			Console.WriteLine("RabbitMQ service started.");
		}

		public void Stop()
		{
			var serviceController = new ServiceController("RabbitMQ", "localhost");
			TimeSpan timeout = TimeSpan.FromMilliseconds(20000);
			Console.WriteLine("Stopping RabbitMQ service ...");
			serviceController.Stop();
			serviceController.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
			Console.WriteLine("RabbitMQ service stopped.");
		}
	}
}