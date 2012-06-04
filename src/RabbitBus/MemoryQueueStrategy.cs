using System.Collections.Generic;

namespace RabbitBus
{
	public class MemoryQueueStrategy : IQueueStrategy
	{
		readonly Queue<MessageInfo> _queue = new Queue<MessageInfo>();

		public int Count { get { return _queue.Count; } }

		public MessageInfo Dequeue()
		{
			return _queue.Dequeue();
		}

		public void Enqueue(MessageInfo messageInfo)
		{
			_queue.Enqueue(messageInfo);
		}
	}
}