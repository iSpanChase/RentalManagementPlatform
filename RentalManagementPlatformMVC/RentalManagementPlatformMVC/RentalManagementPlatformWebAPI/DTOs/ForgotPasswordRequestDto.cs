namespace RentalManagementPlatformWebAPI.DTOs
{
    /// <summary>
    /// 忘記密碼請求用 DTO：
    /// - 前端只需提供 Email
    /// - 後端會依此 Email 產生重設密碼用的 Token 並寄信
    /// </summary>
    public class ForgotPasswordRequestDto
    {
        /// <summary>
        /// 使用者註冊時留下的 Email（用來識別帳號並寄送重設密碼信）
        /// </summary>
        public string Email { get; set; } = null!;
    }
}
