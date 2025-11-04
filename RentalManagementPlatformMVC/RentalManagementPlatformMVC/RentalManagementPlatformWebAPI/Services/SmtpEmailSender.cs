using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace RentalManagementPlatformWebAPI.Services
{
	public class SmtpOptions
	{
		public string Host { get; set; } = "";
		public int Port { get; set; } = 587;                // 587=STARTTLS, 465=SSL
		public string User { get; set; } = "";
		public string Password { get; set; } = "";
		public string FromEmail { get; set; } = "";         // 寄件人信箱
		public string FromName { get; set; } = "AirNest";  // 寄件者名稱
		public bool UseSsl { get; set; } = false;           // 465 時設 true
	}

	public class SmtpEmailSender : IEmailSender
	{
		private readonly SmtpOptions _opt;
		private readonly ILogger<SmtpEmailSender> _logger;

		public SmtpEmailSender(IOptions<SmtpOptions> opt, ILogger<SmtpEmailSender> logger)
		{
			_opt = opt.Value;
			_logger = logger;
		}

		public async Task SendAsync(string toEmail, string subject, string htmlBody, string? toName = null, CancellationToken ct = default)
		{
			if (string.IsNullOrWhiteSpace(_opt.Host)) throw new InvalidOperationException("SMTP Host 未設定");
			if (_opt.Port <= 0) throw new InvalidOperationException("SMTP Port 未正確設定");
			if (string.IsNullOrWhiteSpace(_opt.FromEmail)) throw new InvalidOperationException("寄件者 FromEmail 未設定");

			var msg = new MimeMessage();
			msg.From.Add(new MailboxAddress(_opt.FromName ?? _opt.FromEmail, _opt.FromEmail));
			msg.To.Add(new MailboxAddress(toName ?? toEmail, toEmail));
			msg.Subject = subject;
			msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

			using var client = new SmtpClient();
			try
			{
				if (_opt.UseSsl)
					await client.ConnectAsync(_opt.Host, _opt.Port, SecureSocketOptions.SslOnConnect, ct);
				else
					await client.ConnectAsync(_opt.Host, _opt.Port, SecureSocketOptions.StartTlsWhenAvailable, ct);

				if (!string.IsNullOrWhiteSpace(_opt.User))
					await client.AuthenticateAsync(_opt.User, _opt.Password, ct);

				await client.SendAsync(msg, ct);
				_logger.LogInformation("Email sent to {Email}", toEmail);
			}
			catch (SmtpCommandException sce)
			{
				_logger.LogError(sce, "SMTP command failed: {StatusCode} {Message}", sce.StatusCode, sce.Message);
				throw new InvalidOperationException($"SMTP 指令錯誤（{sce.StatusCode}）：{sce.Message}");
			}
			catch (SmtpProtocolException spe)
			{
				_logger.LogError(spe, "SMTP protocol error: {Message}", spe.Message);
				throw new InvalidOperationException($"SMTP 通訊協定錯誤：{spe.Message}");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "SMTP send failed");
				throw new InvalidOperationException($"SMTP 寄信失敗：{ex.Message}");
			}
			finally
			{
				try { await client.DisconnectAsync(true, ct); } catch { /* ignore */ }
			}
		}
	}
}
