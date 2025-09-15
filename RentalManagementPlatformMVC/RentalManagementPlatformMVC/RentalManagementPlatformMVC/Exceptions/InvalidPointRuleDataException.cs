namespace RentalManagementPlatformMVC.Exceptions
{
	/// <summary>
	/// 點數規則資料驗證失敗時拋出的例外
	/// </summary>
	public class InvalidPointRuleDataException : Exception
	{
		public InvalidPointRuleDataException() : base("點數規則資料無效") { }

		public InvalidPointRuleDataException(string message) : base(message) { }
	}
}