namespace RentalManagementPlatformWebAPI.Services
{
    /// <summary>
    /// 寄送 Email 的介面抽象：
    /// - 讓系統可以在不同環境（SMTP、本地假寄信、第三方服務）有不同實作
    /// - EmailVerificationService 等服務只依賴此介面，不直接綁定特定寄信方式
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// 寄送一封 HTML 格式的 Email
        /// toEmail  : 收件者 Email
        /// subject  : 主旨
        /// htmlBody : HTML 內容
        /// toName   : 收件者顯示名稱（可選）
        /// ct       : CancellationToken
        /// </summary>
        Task SendAsync(string toEmail, string subject, string htmlBody, string? toName = null, CancellationToken ct = default);
    }
}
