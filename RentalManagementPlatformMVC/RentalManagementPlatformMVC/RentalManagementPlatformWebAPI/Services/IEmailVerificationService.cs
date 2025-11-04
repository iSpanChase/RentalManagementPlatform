namespace RentalManagementPlatformWebAPI.Services
{
	public interface IEmailVerificationService
	{
		Task CreateAndSendAsync(int userId, string email, CancellationToken ct, string? baseVerifyUrl = null);
		Task<bool> VerifyAsync(string email, string token, CancellationToken ct);
	}
	public class EmailVerificationOptions
	{
		public int ExpireMinutes { get; set; } = 30;
		public string BaseVerifyUrl { get; set; } = "http://localhost:5173/#/verify-email";
	}
}
