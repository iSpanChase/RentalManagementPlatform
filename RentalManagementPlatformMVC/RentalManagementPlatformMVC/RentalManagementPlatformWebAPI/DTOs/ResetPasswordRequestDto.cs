namespace RentalManagementPlatformWebAPI.DTOs
{
    /// <summary>
    /// 重設密碼請求用 DTO：
    /// - 使用者點擊「忘記密碼」寄來的信後，在前端輸入新密碼
    /// - 前端會把 Token + 新密碼 + （可選）Email 回傳給後端
    /// </summary>
    public class ResetPasswordRequestDto
    {
        /// <summary>
        /// 重設密碼用的 Token：
        /// - 從驗證信連結上帶來
        /// - 後端會用來比對 / 驗證是否有效
        /// </summary>
        public string Token { get; set; } = default!;      // 必填

        /// <summary>
        /// 使用者要設定的新密碼（後端會再做強度驗證與雜湊）
        /// </summary>
        public string NewPassword { get; set; } = default!; // 必填

        /// <summary>
        /// Email（可選）：
        /// - 用於舊 Token 格式或額外安全檢查
        /// - 若 Token 本身不足以識別使用者時，可搭配 Email 使用
        /// </summary>
        public string? Email { get; set; }                  // ← 可選，用於舊 token 的保底
    }
}
