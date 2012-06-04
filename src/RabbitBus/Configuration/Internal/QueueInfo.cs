namespace RabbitBus.Configuration.Internal
{
	class QueueInfo : IQueueInfo, IQueueConfiguration
	{
		public QueueInfo()
		{
			IsAutoDelete = true;
		}

		public IQueueConfiguration AutoDelete()
		{
			IsAutoDelete = true;
			return this;
		}

		public IQueueConfiguration Durable()
		{
			IsDurable = true;
			return this;
		}

		public IQueueConfiguration AutoAcknowledge()
		{
			IsAutoAcknowledge = true;
			return this;
		}

		public IQueueConfiguration UnacknowledgeLimit(ushort count)
		{
			QualityOfService = count;
			return this;
		}

		public ushort QualityOfService { get; set; }

		public INegatableQueueConfiguration Not
		{
			get { return new NegatableQueueConfiguration(this); }
		}

		public bool IsDurable { get; set; }

		public bool IsAutoDelete { get; set; }
		public bool IsAutoAcknowledge { get; set; }

		public class NegatableQueueConfiguration : INegatableQueueConfiguration
		{
			readonly IQueueInfo _queueInfo;

			public NegatableQueueConfiguration(IQueueInfo queueInfo)
			{
				_queueInfo = queueInfo;
			}

			public IQueueConfiguration Durable()
			{
				_queueInfo.IsDurable = !_queueInfo.IsDurable;
				return (IQueueConfiguration) _queueInfo;
			}

			public IQueueConfiguration AutoAcknowledge()
			{
				_queueInfo.IsAutoAcknowledge = false;
				return (IQueueConfiguration) _queueInfo;
			}

			public IQueueConfiguration AutoDelete()
			{
				_queueInfo.IsAutoDelete = false;
				return (IQueueConfiguration) _queueInfo;
			}
		}
	}
}