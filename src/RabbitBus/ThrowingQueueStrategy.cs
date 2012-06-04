using System;

namespace RabbitBus
{
	public class ThrowingQueueStrategy<TException>: IQueueStrategy where TException : Exception, new()
	{
		public int Count
		{
			get { return 0; }
		}

		public MessageInfo Dequeue()
		{
			throw new TException();
		}

		public void Enqueue(MessageInfo messageInfo)
		{
			throw new TException();
		}
	}
}