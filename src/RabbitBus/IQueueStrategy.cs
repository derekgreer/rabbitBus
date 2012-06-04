namespace RabbitBus
{
	public interface IQueueStrategy
	{
		int Count { get; }
		MessageInfo Dequeue();
		void Enqueue(MessageInfo messageInfo);
	}
}