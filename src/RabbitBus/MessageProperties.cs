using System;
using System.Collections;
using System.Collections.Generic;

namespace RabbitBus
{
	public class MessageProperties
	{
		public static MessageProperties Empty = new MessageProperties();

		public TimeSpan? Expiration { get; set; }

		public string RoutingKey { get; set; }

		public IDictionary<string, object> Headers { get; set; }

		protected bool Equals(MessageProperties other)
		{
			return Expiration.Equals(other.Expiration) && string.Equals(RoutingKey, other.RoutingKey) && Equals(Headers, other.Headers);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((MessageProperties) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = Expiration.GetHashCode();
				hashCode = (hashCode*397) ^ (RoutingKey != null ? RoutingKey.GetHashCode() : 0);
				hashCode = (hashCode*397) ^ (Headers != null ? Headers.GetHashCode() : 0);
				return hashCode;
			}
		}

		public static bool operator ==(MessageProperties left, MessageProperties right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(MessageProperties left, MessageProperties right)
		{
			return !Equals(left, right);
		}
	}
}