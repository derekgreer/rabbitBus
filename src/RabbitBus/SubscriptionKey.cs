using System;
using System.Collections;

namespace RabbitBus
{
	class SubscriptionKey : ISubscriptionKey
	{
		readonly Type _messageType;
		readonly MessageProperties _messageProperties;

		public SubscriptionKey(Type messageType, MessageProperties messageProperties)
		{
			_messageType = messageType;
			_messageProperties = messageProperties;
		}

		protected bool Equals(SubscriptionKey other)
		{
			return Equals(_messageType, other._messageType) && Equals(_messageProperties, other._messageProperties);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((SubscriptionKey) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((_messageType != null ? _messageType.GetHashCode() : 0)*397) ^ (_messageProperties != null ? _messageProperties.GetHashCode() : 0);
			}
		}

		public static bool operator ==(SubscriptionKey left, SubscriptionKey right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(SubscriptionKey left, SubscriptionKey right)
		{
			return !Equals(left, right);
		}
	}
}