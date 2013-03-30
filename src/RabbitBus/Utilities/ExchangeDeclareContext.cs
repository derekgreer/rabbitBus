namespace RabbitBus.Utilities
{
	public class ExchangeDeclareContext : IExchangeDeclareContext, IExchangeDeclareInfo
	{
		public ExchangeDeclareContext()
		{
			IsDurable = false;
			IsAutoDelete = true;
		}

		public IExchangeDeclareContext Durable()
		{
			IsDurable = true;
			return this;
		}

		public IExchangeDeclareContext AutoDelete()
		{
			IsAutoDelete = true;
			return this;
		}

		public INegatableExchangeDeclareContext Not
		{
			get { return new NegatableExchangeDeclareContext(this); }
		}

		public string Name { get; set; }
		public string ExchangeType { get; set; }
		public bool IsDurable { get; set; }
		public bool IsAutoDelete { get; set; }
	}

	class NegatableExchangeDeclareContext : INegatableExchangeDeclareContext
	{
		readonly IExchangeDeclareInfo _exchangeDeclareInfo;

		public NegatableExchangeDeclareContext(IExchangeDeclareInfo exchangeDeclareInfo)
		{
			_exchangeDeclareInfo = exchangeDeclareInfo;
		}

		public IExchangeDeclareContext Durable()
		{
			_exchangeDeclareInfo.IsDurable = false;
			return (IExchangeDeclareContext) _exchangeDeclareInfo;
		}

		public IExchangeDeclareContext AutoDelete()
		{
			_exchangeDeclareInfo.IsAutoDelete = false;
			return (IExchangeDeclareContext) _exchangeDeclareInfo;
		}
	}
}