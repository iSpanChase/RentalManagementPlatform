namespace RentalManagementPlatformMVC.Exceptions
{
	/// <summary>
	/// 找不到指定點數規則時拋出的例外
	/// </summary>
	public class PointRuleNotFoundException : Exception
	{
		public PointRuleNotFoundException() : base("找不到指定的點數規則") { }
		public PointRuleNotFoundException(string message) : base(message) { }
	}
}