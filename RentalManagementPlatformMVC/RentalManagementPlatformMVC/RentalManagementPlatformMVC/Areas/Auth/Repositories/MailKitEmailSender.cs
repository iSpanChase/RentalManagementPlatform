using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using RentalManagementPlatformMVC.Areas.Auth.Data;

namespace RentalManagementPlatformMVC.Areas.Auth.Repositories
{
    public class MailKitEmailSender : IEmailSender
    {
		private readonly SmtpOptions _opt;
		public MailKitEmailSender(IOptions<SmtpOptions> opt) => _opt = opt.Value;

		public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
		{
			if (string.IsNullOrWhiteSpace(_opt.From))
				throw new InvalidOperationException("SMTP From address is not configured.");

			var msg = new MimeMessage();
			msg.From.Add(new MailboxAddress(_opt.FromName ?? "", _opt.From));
			msg.To.Add(MailboxAddress.Parse(to));
			msg.Subject = subject ?? "";
			msg.Body = new BodyBuilder { HtmlBody = body ?? "" }.ToMessageBody();

			using var client = new SmtpClient();
			await client.ConnectAsync(_opt.Host, _opt.Port,
				_opt.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, ct);

			// Gmail 需要認證
			await client.AuthenticateAsync(_opt.User, _opt.Password, ct);

			await client.SendAsync(msg, ct);
			await client.DisconnectAsync(true, ct);
		}
	}
}
