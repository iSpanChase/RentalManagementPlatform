using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Meilisearch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace RentalManagementPlatformWebAPI.Services
{
    /// <summary>
    /// Meilisearch 索引更新的背景服務 (消費者)。
    /// 這個服務會繼承 .NET Core 的 BackgroundService，在應用程式啟動時自動在背景執行。
    /// 它的職責是監聽 Redis Stream 中的房源更新訊息，並將這些變動同步到 Meilisearch。
    /// </summary>
    public class MeilisearchIndexWorker : BackgroundService
    {
        // 要監聽的 Redis Stream 名稱，與生產者端一致。
        private const string RoomUpdatesStream = "stream:room-updates";
        // 消費者群組的名稱。使用群組可以讓多個 Worker 實例協作處理訊息，並提供更強的可靠性。
        private const string ConsumerGroup = "meilisearch-workers";
        private readonly ILogger<MeilisearchIndexWorker> _logger;
        // IServiceScopeFactory 用於在 Singleton 生命週期的背景服務中，建立 Scoped 生命週期的服務實例。
        // 因為 DbContext 和我們的多數服務都是 Scoped，所以必須透過它來建立一個新的範圍。
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConnectionMultiplexer _redis;

        public MeilisearchIndexWorker(ILogger<MeilisearchIndexWorker> logger, IServiceScopeFactory scopeFactory, IConnectionMultiplexer redis)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _redis = redis;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Meilisearch 索引更新背景服務已啟動。");

            var db = _redis.GetDatabase();

            // 確保 Redis Stream 和消費者群組存在，如果不存在就建立一個。
            // 這是為了保證服務即使在 Redis 資料被清空或第一次啟動時也能正常運作。
            try
            {
                if (!(await db.KeyExistsAsync(RoomUpdatesStream)) || (await db.StreamGroupInfoAsync(RoomUpdatesStream)).All(g => g.Name != ConsumerGroup))
                {
                    // 從 Stream 的最開頭("0-0")開始建立消費者群組。
                    await db.StreamCreateConsumerGroupAsync(RoomUpdatesStream, ConsumerGroup, "0-0", createStream: true);
                }
            }
            catch (RedisServerException ex) when (ex.Message.Contains("already exists"))
            {
                // 在多個 Worker 同時啟動的競爭情況下，可能會重複建立，此時忽略這個特定的例外。
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 從消費者群組讀取一條尚未被處理的訊息 ('>' 表示新訊息)。
                    // 這是一個阻塞操作，但 Redis 客戶端會有效率地處理它。
                    var entries = await db.StreamReadGroupAsync(RoomUpdatesStream, ConsumerGroup, "worker-1", ">", 1);

                    if (entries.Any())
                    {
                        var entry = entries.First();
                        // 訊息現在以 JSON 字串形式儲存在 "data" 欄位中。
                        var eventJson = entry.Values.FirstOrDefault(v => v.Name == "data").Value;

                        if (!string.IsNullOrEmpty(eventJson))
                        {
                            var roomEvent = JsonSerializer.Deserialize<RoomEventDto>(eventJson);

                            if (roomEvent != null)
                            {
                                _logger.LogInformation("收到需更新索引的訊息，RoomId: {RoomId}, EventType: {EventType}", roomEvent.RoomId, roomEvent.EventType);

                                // 建立一個新的依賴注入範圍(Scope)來解析 Scoped 服務。
                                // 嚴格禁止在 Singleton 服務中直接注入 Scoped 服務！
                                using (var scope = _scopeFactory.CreateScope())
                                {
                                    var queryService = scope.ServiceProvider.GetRequiredService<IRoomListQueryService>();
                                    var meilisearchClient = scope.ServiceProvider.GetRequiredService<MeilisearchClient>();

                                    // 執行實際的索引更新邏輯。
                                    await ProcessRoomEventAsync(roomEvent, queryService, meilisearchClient);
                                }

                                // 處理完畢後，向 Redis 確認(Acknowledge)該訊息已被成功處理。
                                // 這樣這條訊息才不會被同一個群組的其他消費者重複處理。
                                await db.StreamAcknowledgeAsync(RoomUpdatesStream, ConsumerGroup, entry.Id);
                            }
                            else
                            {
                                _logger.LogWarning("無法反序列化 Redis Stream 訊息為 RoomEventDto，訊息 ID: {EntryId}", entry.Id);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Redis Stream 訊息中缺少 'data' 欄位，訊息 ID: {EntryId}", entry.Id);
                        }
                    }
                    else
                    {
                        // 如果沒有新訊息，短暫等待後再繼續輪詢，避免空轉造成 CPU 浪費。
                        await Task.Delay(1000, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "處理 Meilisearch 索引更新時發生錯誤。");
                    // 發生未知錯誤時，等待較長時間再重試，避免在短時間內大量刷錯誤日誌。
                    await Task.Delay(5000, stoppingToken);
                }
            }

            _logger.LogInformation("Meilisearch 索引更新背景服務已停止。");
        }

        /// <summary>
        /// 根據事件類型處理房源的 Meilisearch 索引更新。
        /// </summary>
        private async Task ProcessRoomEventAsync(RoomEventDto roomEvent, IRoomListQueryService queryService, MeilisearchClient meilisearchClient)
        {
            var index = meilisearchClient.Index("rooms");

            switch (roomEvent.EventType)
            {
                case RoomEventType.Created:
                case RoomEventType.Updated:
                case RoomEventType.PhotoAdded:
                    // 對於新增、更新或照片添加事件，從資料庫獲取最新資料並更新索引。
                    var roomDetails = await queryService.GetRoomDataForIndexingAsync(roomEvent.RoomId);
                    if (roomDetails != null)
                    {
                        var roomSearchDto = new RoomListSearchDto
                        {
                            RoomId = roomDetails.RoomId,
                            Title = roomDetails.Title,
                            Description = roomDetails.Description,
                            PricePerNight = roomDetails.PricePerNight,
                            MaxGuests = roomDetails.MaxGuests,
                            HostId = roomDetails.HostId,
                            RatingAvg = roomDetails.RatingAvg,
                            ReviewsCount = roomDetails.ReviewsCount,
                            HostName = roomDetails.Host?.HostName,
                            CityName = roomDetails.CityName,
                            DistrictId = roomDetails.DistrictId,
                            DistrictName = roomDetails.DistrictName,
                            AddressLine = roomDetails.AddressLine,
                            Geo = roomDetails.Geo,
                            CreatedAt = roomDetails.CreatedAt,
                            UpdatedAt = roomDetails.UpdatedAt,
                            Status = roomDetails.Status,
                            IsDeleted = roomDetails.IsDeleted,
                            Amenities = roomDetails.Amenities,
                            CoverBucket = roomDetails.CoverBucket,
                            CoverObjectKey = roomDetails.CoverObjectKey,
                            CoverContentType = roomDetails.CoverContentType,
                            CoverImageUrl = roomDetails.PhotoUrls.FirstOrDefault()
                        };
                        await index.AddDocumentsAsync(new[] { roomSearchDto });
                    }
                    else
                    {
                        // 如果房源不存在 (可能已被刪除)，則嘗試從索引中刪除。
                    await index.DeleteOneDocumentAsync(roomEvent.RoomId.ToString());
                    }
                    break;
                case RoomEventType.Deleted:
                    // 對於刪除事件，直接從 Meilisearch 索引中移除該房源。
                                            await index.DeleteOneDocumentAsync(roomEvent.RoomId.ToString());                    _logger.LogInformation("RoomId {RoomId} 已從 Meilisearch 索引中刪除。", roomEvent.RoomId);
                    break;
                default:
                    _logger.LogWarning("收到未知的 RoomEventType: {EventType} for RoomId: {RoomId}", roomEvent.EventType, roomEvent.RoomId);
                    break;
            }
        }
    }
}
