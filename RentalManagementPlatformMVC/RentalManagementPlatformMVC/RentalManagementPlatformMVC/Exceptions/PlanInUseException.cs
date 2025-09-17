namespace RentalManagementPlatformMVC.Exceptions
{
	public class PlanInUseException : Exception
	{
		public PlanInUseException() : base("該方案目前有訂閱者，無法刪除。") { }
		public PlanInUseException(string message) : base(message) { }
	}
}
