using System.Collections.Generic;

namespace RabbitBus.Configuration.Internal
{
	static class EnumerableExtensions
	{
		public static Queue<T> ToQueue<T>(this IEnumerable<T> collection)
		{
			return new Queue<T>(collection);
		}
	}
}