namespace RentalManagementPlatformWebAPI.DTOs
{
    /// <summary>
    /// 從第三方登入（Google / LINE 等）取得的使用者基本資料：
    /// - 提供給後端用來「找出或建立對應的 User 帳號」
    /// </summary>
    public class ExternalProfileDto
    {
        /// <summary>
        /// 第三方登入提供者名稱：
        /// - 例如 "Google"、"LINE"
        /// </summary>
        public string Provider { get; set; } = "";         // "Google" / "LINE"

        /// <summary>
        /// 第三方提供者的使用者唯一識別：
        /// - Google 的 sub
        /// - LINE 的 userId
        /// - 會搭配 Provider 一起當成綁定 key
        /// </summary>
        public string ProviderUserId { get; set; } = "";   // Google sub 或 LINE userId

        /// <summary>
        /// 第三方回傳的 Email（可能為 null，例如某些 LINE 帳號不提供 email）
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 顯示名稱（暱稱或姓名），視第三方回傳內容而定
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 頭像圖片的 URL（若第三方有提供）
        /// </summary>
        public string? PictureUrl { get; set; }
    }
}
