using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using RentalManagementPlatformWebAPI.DTOs;
using Microsoft.AspNetCore.StaticFiles;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using System.IO;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace RentalManagementPlatformWebAPI.Services
{
    /// <summary>
    /// MinIO 儲存服務的實作，負責檔案的上下傳和 URL 取得。
    /// </summary>
    public class MinioService : IMinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly MinioSettings _minioSettings;
        private readonly ILogger<MinioService> _logger;

        public MinioService(IOptions<MinioSettings> minioOptions, ILogger<MinioService> logger)
        {
            _minioSettings = minioOptions.Value;
            _logger = logger;

            // 根據設定檔中的 UseSsl 決定是否使用 HTTPS 連線。
            // 這是為了在開發和生產環境中提供彈性。
            var minioClientBuilder = new MinioClient()
                .WithEndpoint(_minioSettings.Endpoint)
                .WithCredentials(_minioSettings.AccessKey, _minioSettings.SecretKey);

            if (_minioSettings.UseSsl)
            {
                minioClientBuilder = minioClientBuilder.WithSSL();
            }

            _minioClient = minioClientBuilder.Build();
        }

        /// <summary>
        /// 上傳檔案到 MinIO 儲存桶。
        /// </summary>
        /// <param name="stream">檔案的內容串流。</param>
        /// <param name="fileName">檔案在 MinIO 中儲存的名稱 (ObjectKey)。</param>
        /// <returns>成功上傳後回傳檔案名稱 (ObjectKey)。</returns>
        public async Task<string> UploadFileAsync(Stream stream, string fileName)
        {
            try
            {
                var bucketName = _minioSettings.BucketName;

                // 檢查儲存桶是否存在，如果不存在則建立。
                var beArgs = new BucketExistsArgs().WithBucket(bucketName);
                if (!await _minioClient.BucketExistsAsync(beArgs))
                {
                    var mbArgs = new MakeBucketArgs().WithBucket(bucketName);
                    await _minioClient.MakeBucketAsync(mbArgs);
                }

                // 嘗試根據檔案副檔名獲取 ContentType，如果無法獲取則使用預設值。
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(fileName, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                // 準備上傳物件的參數。
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(contentType);

                // 執行上傳操作。
                await _minioClient.PutObjectAsync(putObjectArgs);

                return fileName;
            }
            catch (Exception ex)
            {
                // 使用 ILogger 記錄錯誤，而不是 Console.WriteLine。
                _logger.LogError(ex, "上傳檔案到 MinIO 時發生錯誤: {FileName}", fileName);
                throw; // 重新拋出例外，讓上層處理。
            }
        }

        /// <summary>
        /// 取得檔案的預簽章 URL，該 URL 在一段時間內有效。
        /// </summary>
        /// <param name="objectName">檔案在 MinIO 中儲存的名稱 (ObjectKey)。</param>
        /// <returns>檔案的預簽章 URL。</returns>
        public async Task<string> GetFileUrlAsync(string objectName)
        {
            try
            {
                var bucketName = _minioSettings.BucketName;
                // 從設定檔中取得 URL 的有效期限，而不是硬編碼。
                var expiry = _minioSettings.UrlExpirySeconds;
                if (expiry < 1)
                {
                    expiry = 3600; // Default to 1 hour if not configured or invalid
                }

                var args = new PresignedGetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithExpiry(expiry);

                return await _minioClient.PresignedGetObjectAsync(args);
            }
            catch (Exception ex)
            {
                // 使用 ILogger 記錄錯誤。
                _logger.LogError(ex, "從 MinIO 取得檔案 URL 時發生錯誤: {ObjectName}", objectName);
                throw; // 重新拋出例外。
            }
        }
    }
}