using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace RentalManagementPlatformWebAPI.Services
{
    /// <summary>
    /// SMTP 寄信設定：
    /// - Host / Port / User / Password 為連線資訊
    /// - FromEmail / FromName 為寄件者資訊
    /// - UseSsl 控制是否使用 SSL（465 通常為 SSL）
    /// </summary>
    public class SmtpOptions
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;                // 587=STARTTLS, 465=SSL
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
        public string FromEmail { get; set; } = "";         // 寄件人信箱
        public string FromName { get; set; } = "AirNest";   // 寄件者名稱
        public bool UseSsl { get; set; } = false;           // 465 時設 true
    }

    /// <summary>
    /// 以 MailKit + SMTP 實作 IEmailSender：
    /// - 根據 SmtpOptions 建立 SMTP 連線
    /// - 使用 MimeMessage 組合 HTML 郵件內容
    /// - 發送失敗會記錄 log 並包裝為 InvalidOperationException 對外丟出
    /// </summary>
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _opt;
        private readonly ILogger<SmtpEmailSender> _logger;

        /// <summary>
        /// 透過 IOptions 取得 SMTP 設定，並注入 logger
        /// </summary>
        public SmtpEmailSender(IOptions<SmtpOptions> opt, ILogger<SmtpEmailSender> logger)
        {
            _opt = opt.Value;
            _logger = logger;
        }

        /// <summary>
        /// 寄送一封 HTML Email：
        /// 1. 檢查 SMTP 設定是否完整（Host/Port/FromEmail）
        /// 2. 使用 MimeMessage/MailboxAddress 組出郵件
        /// 3. 使用 MailKit 的 SmtpClient 連線、（必要時）驗證帳號、送出郵件
        /// 4. 發送過程若有任何 SMTP 相關例外，記錄 log 並轉成較易閱讀的錯誤訊息
        /// </summary>
        public async Task SendAsync(string toEmail, string subject, string htmlBody, string? toName = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_opt.Host)) throw new InvalidOperationException("SMTP Host 未設定");
            if (_opt.Port <= 0) throw new InvalidOperationException("SMTP Port 未正確設定");
            if (string.IsNullOrWhiteSpace(_opt.FromEmail)) throw new InvalidOperationException("寄件者 FromEmail 未設定");

            // 建立郵件內容
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_opt.FromName ?? _opt.FromEmail, _opt.FromEmail));
            msg.To.Add(new MailboxAddress(toName ?? toEmail, toEmail));
            msg.Subject = subject;
            msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                // 依 UseSsl 決定使用 SSL 或 STARTTLS
                if (_opt.UseSsl)
                    await client.ConnectAsync(_opt.Host, _opt.Port, SecureSocketOptions.SslOnConnect, ct);
                else
                    await client.ConnectAsync(_opt.Host, _opt.Port, SecureSocketOptions.StartTlsWhenAvailable, ct);

                // 若有設定帳號密碼則進行驗證
                if (!string.IsNullOrWhiteSpace(_opt.User))
                    await client.AuthenticateAsync(_opt.User, _opt.Password, ct);

                // 實際送出郵件
                await client.SendAsync(msg, ct);
                _logger.LogInformation("Email sent to {Email}", toEmail);
            }
            catch (SmtpCommandException sce)
            {
                // SMTP 指令錯誤（例如收件者不存在、帳號被拒等）
                _logger.LogError(sce, "SMTP command failed: {StatusCode} {Message}", sce.StatusCode, sce.Message);
                throw new InvalidOperationException($"SMTP 指令錯誤（{sce.StatusCode}）：{sce.Message}");
            }
            catch (SmtpProtocolException spe)
            {
                // SMTP 通訊協定錯誤（例如握手失敗、封包錯亂等）
                _logger.LogError(spe, "SMTP protocol error: {Message}", spe.Message);
                throw new InvalidOperationException($"SMTP 通訊協定錯誤：{spe.Message}");
            }
            catch (Exception ex)
            {
                // 其他非預期錯誤
                _logger.LogError(ex, "SMTP send failed");
                throw new InvalidOperationException($"SMTP 寄信失敗：{ex.Message}");
            }
            finally
            {
                // 無論成功與否，都嘗試正常關閉連線
                try { await client.DisconnectAsync(true, ct); } catch { /* ignore */ }
            }
        }
    }
}
