namespace RentalManagementPlatformMVC.Exceptions
{
	/// <summary>
	/// 點數規則日期範圍無效時拋出的例外
	/// </summary>
	public class PointRuleDateRangeException : Exception
	{
		public PointRuleDateRangeException() : base("點數規則日期範圍無效") { }
		public PointRuleDateRangeException(string message) : base(message) { }
	}
}