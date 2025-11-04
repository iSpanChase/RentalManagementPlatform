namespace RentalManagementPlatformWebAPI.Services
{
	public interface IEmailSender
	{
		Task SendAsync(string toEmail, string subject, string htmlBody, string? toName = null, CancellationToken ct = default);
	}
}
