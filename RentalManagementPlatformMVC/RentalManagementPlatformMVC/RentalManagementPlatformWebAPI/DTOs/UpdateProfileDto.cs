namespace RentalManagementPlatformWebAPI.DTOs
{
    /// <summary>
    /// 更新個人資料用 DTO：
    /// - 前端 Profile 編輯頁送出的表單資料
    /// - 後端會依此更新 User 實體的對應欄位
    /// </summary>
    public class UpdateProfileDto
    {
        // NOT NULL 欄位（請依 DB 規則）
        /// <summary>
        /// 姓名 / 顯示名稱（必填）
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// 性別欄位（必填，依 DB 規則）
        /// </summary>
        public string Gender { get; set; } = null!;

        /// <summary>
        /// 生日：
        /// - 前端傳 yyyy-MM-dd
        /// - model binder 解析為 DateTime
        /// </summary>
        // 後端：接 yyyy-MM-dd，model binder 會解析為 DateTime
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// 通訊地址（必填）
        /// </summary>
        public string Address { get; set; } = null!;

        // 可為 NULL 的欄位

        /// <summary>
        /// 聯絡電話（選填）
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 點數欄位（選填），例如會員點數／紅利
        /// </summary>
        public int? Point { get; set; }

        /// <summary>
        /// 頭像圖片 URL（選填），通常由上傳 API 產生
        /// </summary>
        public string? ProfileImageUrl { get; set; }
    }
}
