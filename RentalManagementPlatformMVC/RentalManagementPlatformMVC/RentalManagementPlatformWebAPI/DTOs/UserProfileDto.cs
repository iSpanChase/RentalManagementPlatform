namespace RentalManagementPlatformWebAPI.DTOs
{
    /// <summary>
    /// 對前端公開的「使用者個人資料」 DTO：
    /// - 登入回傳、取得個人資料、FAQ 顯示等都會用到這個模型
    /// - 只包含前端需要的欄位，不直接曝露完整 User 實體
    /// </summary>
    public class UserProfileDto
    {
        /// <summary>
        /// 使用者主鍵 Id（對應 USER.UserId）
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 帳號名稱（前端顯示用）
        /// </summary>
        public string Username { get; set; } = "";

        /// <summary>
        /// 使用者 Email（登入帳號）
        /// </summary>
        public string Email { get; set; } = "";

        /// <summary>
        /// 顯示姓名（真實姓名或暱稱）
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// 性別欄位
        /// </summary>
        public string Gender { get; set; } = "";

        /// <summary>
        /// 生日（DateTime），前端通常以 yyyy-MM-dd 顯示/編輯
        /// </summary>
        public DateTime BirthDate { get; set; }   // 前端以 yyyy-MM-dd 顯示/編輯

        /// <summary>
        /// 聯絡電話
        /// </summary>
        public string Phone { get; set; } = "";

        /// <summary>
        /// 通訊地址
        /// </summary>
        public string Address { get; set; } = "";

        /// <summary>
        /// 點數（可為 null），例如會員點數
        /// </summary>
        public int? Point { get; set; }

        /// <summary>
        /// 大頭照 / 頭像圖片的 URL
        /// </summary>
        public string? ProfileImageUrl { get; set; }

        /// <summary>
        /// 是否已完成 Email 驗證（對應 USER.isverified）
        /// </summary>
        public bool IsVerified { get; set; }      // 對應 USER.isverified
    }
}
