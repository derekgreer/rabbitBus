namespace RabbitBus.Configuration.Internal
{
	class ExchangeInfo : IExchangeInfo, IExchangeConfiguration
	{
		public ExchangeInfo()
		{
			IsAutoDelete = true;
			ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
		}

		protected string RoutingKey { get; private set; }

		public INegatableExchangeConfiguration Not
		{
			get { return new NegatableExchangeConfiguration(this); }
		}

		public IExchangeConfiguration AutoDelete()
		{
			IsAutoDelete = true;
			return this;
		}

		public IExchangeConfiguration Durable()
		{
			IsDurable = true;
			return this;
		}

		public IExchangeConfiguration Fanout()
		{
			ExchangeType = RabbitMQ.Client.ExchangeType.Fanout;
			return this;
		}

		public IExchangeConfiguration Direct()
		{
			ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
			return this;
		}

		public IExchangeConfiguration Headers()
		{
			ExchangeType = RabbitMQ.Client.ExchangeType.Headers;
			return this;
		}

		public IExchangeConfiguration Topic()
		{
			ExchangeType = RabbitMQ.Client.ExchangeType.Topic;
			return this;
		}

		public bool IsDurable { get; set; }

		public string ExchangeType { get; set; }

		public bool IsAutoDelete { get; set; }

		public class NegatableExchangeConfiguration : INegatableExchangeConfiguration
		{
			readonly IExchangeInfo _exchangeInfo;

			public NegatableExchangeConfiguration(IExchangeInfo exchangeInfo)
			{
				_exchangeInfo = exchangeInfo;
			}

			public IExchangeConfiguration Durable()
			{
				_exchangeInfo.IsDurable = !_exchangeInfo.IsDurable;
				return (IExchangeConfiguration) _exchangeInfo;
			}

			public IExchangeConfiguration AutoDelete()
			{
				_exchangeInfo.IsAutoDelete = !_exchangeInfo.IsAutoDelete;
				return (IExchangeConfiguration) _exchangeInfo;
			}
		}
	}
}