using System;
using System.Collections;

namespace RabbitBus
{
	class SubscriptionKey : ISubscriptionKey
	{
		readonly Type _messageType;
		readonly string _routingKey;
		readonly IDictionary _arguments;

		public SubscriptionKey(Type messageType, string routingKey, IDictionary arguments)
		{
			_messageType = messageType;
			_routingKey = routingKey;
			_arguments = arguments;
		}

		public bool Equals(SubscriptionKey other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other._messageType, _messageType) && Equals(other._routingKey, _routingKey) && Equals(other._arguments, _arguments);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (SubscriptionKey)) return false;
			return Equals((SubscriptionKey) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (_messageType != null ? _messageType.GetHashCode() : 0);
				result = (result*397) ^ (_routingKey != null ? _routingKey.GetHashCode() : 0);
				result = (result*397) ^ (_arguments != null ? _arguments.GetHashCode() : 0);
				return result;
			}
		}
	}
}