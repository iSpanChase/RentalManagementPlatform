namespace RentalManagementPlatformMVC.Exceptions
{
	public class PlanNotFoundException : Exception
	{
		public PlanNotFoundException() : base("找不到指定的方案") { }
		public PlanNotFoundException(string message) : base(message) { }
	}
}
