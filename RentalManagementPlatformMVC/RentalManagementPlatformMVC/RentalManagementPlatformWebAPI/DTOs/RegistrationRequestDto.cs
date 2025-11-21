namespace RentalManagementPlatformWebAPI.DTOs
{
    /// <summary>
    /// 註冊帳號用的 DTO：
    /// - 前端把註冊畫面填寫的資料打包丟到後端
    /// - 後端會依 RoleCode 決定角色 / 是否為待審核系統管理員等
    /// </summary>
    public class RegistrationRequestDto
    {
        /// <summary>
        /// 角色代碼：
        /// - 例如 LANDLORD / TENANT / VENDOR / ADMIN / OPERATOR ...
        /// - 後端會依照這個代碼指派角色或標記為待審核
        /// </summary>
        public string RoleCode { get; set; } = null!; // LANDLORD/TENANT/VENDOR/ADMIN

        /// <summary>
        /// 註冊使用的 Email（作為登入帳號，需唯一）
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// 目前命名為 PasswordHash，但實際上前端傳原始密碼字串：
        /// - 後端會再用 PasswordHasher 雜湊後寫入 DB
        /// </summary>
        public string PasswordHash { get; set; } = null!;

        /// <summary>
        /// 顯示姓名（姓名或暱稱）
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// 帳號名稱（Username），通常顯示在前台，例如 airbnb 的暱稱
        /// </summary>
        public string Username { get; set; } = null!;

        /// <summary>
        /// 聯絡電話（可允許空字串）
        /// </summary>
        public string Phone { get; set; } = "";

        // 新增：註冊即收齊
        /// <summary>
        /// 性別欄位（依專案規則可能為 "M"/"F" 或其他字串）
        /// </summary>
        public string Gender { get; set; } = null!;

        /// <summary>
        /// 生日：
        /// - 前端以 yyyy-MM-dd 傳入
        /// - ASP.NET model binder 會自動轉成 DateTime
        /// </summary>
        public DateTime BirthDate { get; set; }      // 前端傳 yyyy-MM-dd，model binder 可解析

        /// <summary>
        /// 通訊地址（必填）
        /// </summary>
        public string Address { get; set; } = null!;

        /// <summary>
        /// 大頭照 / 頭像圖片的 URL（若註冊時就有上傳）
        /// </summary>
        public string? ProfileImageUrl { get; set; }

        // 依角色擴充欄位（範例）
        /// <summary>
        /// 公司名稱（例如房東公司、廠商等，角色相關欄位）
        /// </summary>
        public string? CompanyName { get; set; }

        /// <summary>
        /// 統一編號 / 稅籍編號（角色相關欄位）
        /// </summary>
        public string? TaxId { get; set; }

        /// <summary>
        /// 身分證字號後幾碼（視專案需求使用，角色相關欄位）
        /// </summary>
        public string? NationalIdTail { get; set; }
    }
}
