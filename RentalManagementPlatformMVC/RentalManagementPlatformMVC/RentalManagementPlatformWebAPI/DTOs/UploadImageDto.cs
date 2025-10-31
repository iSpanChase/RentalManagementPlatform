using Microsoft.AspNetCore.Http;

namespace RentalManagementPlatformWebAPI.DTOs
{
    /// <summary>
    /// 圖片上傳的資料傳輸物件 (DTO)。
    /// 用於接收前端透過 multipart/form-data 形式上傳的圖片檔案及其相關中繼資料。
    /// 由於圖片上傳是通用功能，不限於房源，因此將其放置在 DTOs 根目錄下。
    /// </summary>
    public class UploadImageDto
    {
        /// <summary>
        /// 上傳的圖片檔案本身。IFormFile 是 ASP.NET Core 用於處理上傳檔案的介面。
        /// 標記為可為 null (?) 以符合 C# 8.0 的 Nullable Reference Types 規範，並避免 CS8618 警告。
        /// </summary>
        public IFormFile? ImageFile { get; set; }

        /// <summary>
        /// 圖片的類型標籤，例如 "Cover" (封面), "Profile" (大頭貼), "Gallery" (圖庫) 等。
        /// 用於區分圖片的用途。預設值為 string.Empty，以避免 CS8618 警告。
        /// </summary>
        public string PhotoType { get; set; } = string.Empty;
    }
}
