namespace RentalManagementPlatformMVC.Exceptions
{
	/// <summary>
	/// 點數規則正在使用中無法執行操作時拋出的例外
	/// </summary>
	public class PointRuleInUseException : Exception
	{
		public PointRuleInUseException() : base("點數規則正在使用中") { }
		public PointRuleInUseException(string message) : base(message) { }
	}
}