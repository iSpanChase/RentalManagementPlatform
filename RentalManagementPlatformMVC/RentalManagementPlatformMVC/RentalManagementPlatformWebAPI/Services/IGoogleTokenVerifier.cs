namespace RentalManagementPlatformWebAPI.Services
{
    /// <summary>
    /// 對 Google ID Token 進行驗證的介面：
    /// - 抽象出驗證動作，方便日後替換實作或寫單元測試（可用假實作）
    /// </summary>
    public interface IGoogleTokenVerifier
    {
        /// <summary>
        /// 驗證 Google 的 idToken：
        /// - expectedAudience：預期的 aud（通常是你的 Google ClientId），可為 null
        /// - 驗證成功回傳 GoogleProfile；失敗回傳 null
        /// </summary>
        Task<GoogleProfile?> VerifyAsync(string idToken, string? expectedAudience = null);
    }

    /// <summary>
    /// Google 驗證成功後萃取出來的使用者資料：
    /// - Sub           : Google 帳號在此專案下的唯一識別（subject）
    /// - Email         : 使用者 Email
    /// - Name          : 顯示名稱（可能為 null）
    /// - Picture       : 頭像 URL（可能為 null）
    /// - EmailVerified : Email 是否已通過 Google 驗證
    /// </summary>
    public record GoogleProfile(string Sub, string Email, string? Name, string? Picture, bool EmailVerified);
}
