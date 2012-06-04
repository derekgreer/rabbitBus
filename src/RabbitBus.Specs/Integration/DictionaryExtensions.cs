using System.Collections;
using System.Collections.Generic;

namespace RabbitBus.Specs.Integration
{
	public static class DictionaryExtensions
	{
		public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IDictionary dictionary)
		{
			IDictionary<TKey, TValue> newDictionary = new Dictionary<TKey, TValue>();
			foreach (TKey key in dictionary.Keys)
			{
				newDictionary.Add(key, (TValue) dictionary[key]);
			}
			return newDictionary;
		}
	}
}