namespace RentalManagementPlatformWebAPI.Services
{
    /// <summary>
    /// Email 驗證服務介面：
    /// - 建立驗證碼並寄送驗證信
    /// - 驗證使用者點擊連結帶來的 email + token 是否有效
    /// </summary>
    public interface IEmailVerificationService
    {
        /// <summary>
        /// 建立新的 Email 驗證紀錄並寄出 Email
        /// userId       : 要驗證的使用者 ID
        /// email        : 寄送目標 Email
        /// baseVerifyUrl: 前端驗證頁的基底網址（可選，未給則使用設定檔）
        /// </summary>
        Task CreateAndSendAsync(int userId, string email, CancellationToken ct, string? baseVerifyUrl = null);

        /// <summary>
        /// 驗證 email + token 是否有效：
        /// - 驗證成功會順便將該使用者標記為已驗證
        /// - 回傳 true/false 讓呼叫端決定顯示訊息
        /// </summary>
        Task<bool> VerifyAsync(string email, string token, CancellationToken ct);
    }

    /// <summary>
    /// Email 驗證相關設定值：
    /// - ExpireMinutes: 驗證碼有效分鐘數
    /// - BaseVerifyUrl: 前端驗證頁的基底 URL
    ///   例如 http://localhost:5173/#/verify-email
    /// </summary>
    public class EmailVerificationOptions
    {
        public int ExpireMinutes { get; set; } = 30;
        public string BaseVerifyUrl { get; set; } = "http://localhost:5173/#/verify-email";
    }
}
