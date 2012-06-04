using Machine.Specifications;

namespace RabbitBus.Specs.Infrastructure
{
	public class IntegrationAttribute : TagsAttribute
	{
		public IntegrationAttribute() : base("integration")
		{
		}
	}
}