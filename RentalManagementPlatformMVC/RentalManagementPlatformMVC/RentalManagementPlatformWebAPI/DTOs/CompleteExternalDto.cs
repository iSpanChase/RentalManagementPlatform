namespace RentalManagementPlatformWebAPI.DTOs
{
    /// <summary>
    /// 第三方登入帳號「補資料」用的 DTO：
    /// - 例如 Google / LINE 登入後，原本帳號資料不完整時，
    ///   前端可以用這個 DTO 把 Email / Phone / DisplayName 補齊
    /// </summary>
    public class CompleteExternalDto
    {
        /// <summary>
        /// 使用者要補上的正式 Email（必填）：
        /// - 後端會檢查是否已被其他帳號使用
        /// - 通常會將此 Email 標為已驗證或再寄驗證信
        /// </summary>
        public string Email { get; set; } = default!;

        /// <summary>
        /// 使用者的聯絡電話（選填）
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 使用者顯示名稱（選填），例如暱稱或真實姓名
        /// </summary>
        public string? DisplayName { get; set; }
    }
}
