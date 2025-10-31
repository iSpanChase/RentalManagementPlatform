namespace RentalManagementPlatformWebAPI.DTOs
{
    /// <summary>
    /// MinIO 儲存服務的設定物件 (DTO)。
    /// 用於從 appsettings.json 讀取 MinIO 的連線和操作相關配置。
    /// 由於 MinIO 服務是通用基礎設施，不限於特定業務邏輯，因此將其放置在 DTOs 根目錄下。
    /// </summary>
    public class MinioSettings
    {
        /// <summary>
        /// MinIO 服務的端點 (Endpoint)，例如 "127.0.0.1:9000" 或 "s3.amazonaws.com"。
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// MinIO 存取金鑰 (Access Key)。
        /// </summary>
        public string AccessKey { get; set; } = string.Empty;

        /// <summary>
        /// MinIO 秘密金鑰 (Secret Key)。
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// 儲存桶名稱 (Bucket Name)，用於存放檔案。
        /// </summary>
        public string BucketName { get; set; } = string.Empty;

        /// <summary>
        /// 是否使用 SSL/TLS 連線到 MinIO 服務。建議在生產環境中啟用。
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// 預簽章 URL 的有效期限 (秒)。用於生成臨時可訪問的檔案連結。
        /// </summary>
        public int UrlExpirySeconds { get; set; }
    }
}