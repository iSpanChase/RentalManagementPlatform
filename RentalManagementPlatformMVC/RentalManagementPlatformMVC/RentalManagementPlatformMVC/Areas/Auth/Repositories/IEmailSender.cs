namespace RentalManagementPlatformMVC.Areas.Auth.Repositories
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string htmlOrTextBody, CancellationToken ct = default);
    }
}
