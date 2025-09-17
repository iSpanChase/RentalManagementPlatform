namespace RentalManagementPlatformMVC.Exceptions
{
	public class PlanNameExistException : Exception
	{
		public PlanNameExistException() : base("方案名稱已存在") { }
		public PlanNameExistException(string message) : base(message) { }
	}
}
