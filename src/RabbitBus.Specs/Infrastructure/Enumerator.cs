using System;
using System.Collections.Generic;

namespace RabbitBus.Specs.Infrastructure
{
	public static class Enumerator
	{
		public static IEnumerable<T> Enumerate<T>(Func<T> func) where T : class
		{
			while (true)
			{
				T item = func();
				if (item == null)
					break;
				yield return item;
			}
		}
	}
}