namespace RentalManagementPlatformMVC.Areas.UserManagement.Module
{
	public sealed class OpResult
	{
		public bool Succeeded { get; private set; }
		public string? Code { get; private set; }
		public string? Message { get; private set; }

		public static OpResult Ok() => new OpResult { Succeeded = true };
		public static OpResult Fail(string code, string message) => new OpResult { Succeeded = false, Code = code, Message = message };
	}
}
