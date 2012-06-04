using System;

namespace RabbitBus.Specs.Infrastructure
{
	public static class ObjectExtensions
	{
		public static T ProvideDefault<T>(this T instance, Func<T> factory) where T : class
		{
			instance = instance ?? factory();
			return instance;
		}

		public static T ProvideDefault<T>(this T instance) where T : class, new()
		{
			return instance ?? new T();
		}
	}
}