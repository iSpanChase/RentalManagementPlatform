using Meilisearch;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.DTOs; // API's DTOs namespace
using RentalManagementPlatformWebAPI.Models; // API's Models namespace
using RentalManagementPlatformWebAPI.Repositories.Interfaces; // API's Repositories.Interfaces namespace
using RentalManagementPlatformWebAPI.Services.Interfaces; // API's Services.Interfaces namespace
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Text.Json;
using Microsoft.Extensions.Logging; // Added for ILogger

namespace RentalManagementPlatformWebAPI.Services
{
    public class MeilisearchService
    {
        private readonly MeilisearchClient _meiliClient;
        private readonly RentalManagementPlatformSqlContext _dbContext; // Changed to API's DbContext
        private readonly ILogger<MeilisearchService> _logger;
        private readonly IMinioService _minioService; // Assuming IMinioService will be in API's Services.Interfaces
        private const string IndexName = "rooms";
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public MeilisearchService(
            MeilisearchClient meiliClient,
            RentalManagementPlatformSqlContext dbContext,
            ILogger<MeilisearchService> logger,
            IMinioService minioService)
        {
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true
            };
            _meiliClient = meiliClient;

            _dbContext = dbContext;
            _logger = logger;
            _minioService = minioService;
        }

        public async Task<IEnumerable<RoomListSearchDto>> SearchAsync(string query, string? status = null)
        {
            var filters = new List<string>();

            if (!string.IsNullOrWhiteSpace(status))
                filters.Add($"status = \"{status}\"");

            try
            {
                _logger.LogInformation("Searching Meilisearch index '{IndexName}' with query: '{Query}'.", IndexName, query);
                var index = _meiliClient.Index(IndexName);
                var searchResult = await index.SearchAsync<RoomListSearchDto>(query, new SearchQuery
                {
                    Filter = filters.Count > 0 ? string.Join(" AND ", filters) : null,
                    Limit = 200,
                });

                _logger.LogInformation("Meilisearch raw response: {RawResponse}", System.Text.Json.JsonSerializer.Serialize(searchResult));

                var hits = searchResult.Hits.ToList();
                _logger.LogInformation("Meilisearch returned {Count} hits.", hits.Count);

                foreach (var hit in hits)
                {
                    _logger.LogInformation("DEBUG: RoomId={RoomId}, Title='{Title}', IsDeleted={IsDeleted}", hit.RoomId, hit.Title, hit.IsDeleted);
                }

                foreach (var hit in hits)
                {
                    if (!string.IsNullOrEmpty(hit.CoverObjectKey))
                    {
                        try
                        {
                            hit.CoverImageUrl = await _minioService.GetFileUrlAsync(hit.CoverObjectKey);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to get MinIO URL for object key '{ObjectKey}'.", hit.CoverObjectKey);
                            hit.CoverImageUrl = null;
                        }
                    }
                }

                if (hits.Any())
                {
                    var roomIds = hits.Select(h => h.RoomId)
                        .Where(id => id > 0)
                        .Distinct()
                        .ToList();

                    if (roomIds.Any())
                    {
                        var ratingStats = await _dbContext.Reviews
                            .Where(review => review.RoomId.HasValue && roomIds.Contains(review.RoomId.Value) && review.Rating.HasValue)
                            .GroupBy(review => review.RoomId!.Value)
                            .Select(group => new
                            {
                                RoomId = group.Key,
                                AverageRating = group.Average(review => review.Rating!.Value),
                                ReviewsCount = group.Count()
                            })
                            .ToDictionaryAsync(group => group.RoomId);

                        foreach (var hit in hits)
                        {
                            if (ratingStats.TryGetValue(hit.RoomId, out var stats))
                            {
                                hit.RatingAvg = Math.Round(stats.AverageRating, 1, MidpointRounding.AwayFromZero);
                                hit.ReviewsCount = stats.ReviewsCount;
                            }
                            else
                            {
                                hit.RatingAvg = 0;
                                hit.ReviewsCount = 0;
                            }
                        }
                    }
                    else
                    {
                        foreach (var hit in hits)
                        {
                            hit.RatingAvg = 0;
                            hit.ReviewsCount = 0;
                        }
                    }

                    var firstHit = hits.First();
                    _logger.LogInformation("First hit details: RoomId={RoomId}, Title='{Title}', CoverImageUrl='{CoverImageUrl}', RatingAvg={RatingAvg}, ReviewsCount={ReviewsCount}",
                        firstHit.RoomId, firstHit.Title, firstHit.CoverImageUrl, firstHit.RatingAvg, firstHit.ReviewsCount);
                }
                return hits;
            }
            catch (Exception ex)
            {
                Console.WriteLine("!!!!!!!!!! CAUGHT EXCEPTION IN MeilisearchService !!!!!!!!!!");
                Console.WriteLine(ex.ToString());
                _logger.LogError(ex, "An error occurred while searching Meilisearch index '{IndexName}'.", IndexName);
                return Enumerable.Empty<RoomListSearchDto>();
            }
        }

        public async Task IndexAllRoomListsAsync()
        {
            try
            {
                _logger.LogInformation("Starting to index all room lists.");
                var index = _meiliClient.Index(IndexName);
                const int batchSize = 1000;
                int totalRooms = await _dbContext.RoomLists.CountAsync();

                _logger.LogInformation("Total rooms to index: {TotalRooms}", totalRooms);

                for (int skip = 0; skip < totalRooms; skip += batchSize)
                {
                    _logger.LogInformation("Processing batch: Skip {Skip}, Take {Take}", skip, batchSize);
                    var dtos = await GetRoomListDtosBatchAsync(skip, batchSize);

                    if (dtos.Any())
                    {
                        try
                        {
                            var taskInfo = await index.AddDocumentsAsync(dtos);
                            _logger.LogInformation("Successfully sent {Count} documents to Meilisearch. Task ID: {TaskId}", dtos.Count, taskInfo.TaskUid);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send {Count} documents to Meilisearch for indexing in batch (Skip: {Skip}, Take: {Take}).", dtos.Count, skip, batchSize);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("No documents found in batch (Skip: {Skip}, Take: {Take}).", skip, batchSize);
                    }
                }
                _logger.LogInformation("Finished indexing all room lists.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to index all documents.");
            }
        }

        public async Task AddOrUpdateDocumentAsync(int roomId)
        {
            try
            {
                var dto = await MapSingleToDto(roomId);
                if (dto == null)
                {
                    _logger.LogWarning("Room with ID {RoomId} not found or could not be mapped for indexing.", roomId);
                    return;
                }

                var index = _meiliClient.Index(IndexName);
                await index.AddDocumentsAsync(new[] { dto });
                _logger.LogInformation("Successfully indexed document for Room ID {RoomId}.", roomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add or update document for Room ID {RoomId}.", roomId);
            }
        }

        public async Task DeleteDocumentAsync(int roomId)
        {
            try
            {
                var index = _meiliClient.Index(IndexName);
                var task = await index.DeleteOneDocumentAsync(roomId.ToString());
                _logger.LogInformation("Successfully requested deletion for document Room ID {RoomId}. Task ID: {TaskId}", roomId, task.TaskUid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete document for Room ID {RoomId}.", roomId);
            }
        }

        private async Task<RoomListSearchDto?> MapSingleToDto(int roomId)
        {
            var result = await (from room in _dbContext.RoomLists.Include(r => r.RoomPhotos)
                                where room.RoomId == roomId
                                join user in _dbContext.Users on room.HostId equals user.UserId
                                join address in _dbContext.Addresses on room.AddressId equals address.AddressId
                                join district in _dbContext.Districts on address.DistrictId equals district.DistrictId
                                join city in _dbContext.Cities on district.CityId equals city.CityId
                                select new
                                {
                                    room, user, address, district, city,
                                    CoverPhoto = room.RoomPhotos.OrderBy(p => p.SortOrder).FirstOrDefault(p => p.PhotoType == "Cover") ??
                                                 room.RoomPhotos.OrderBy(p => p.SortOrder).FirstOrDefault()
                                })
                                .FirstOrDefaultAsync();

            if (result == null) return null;

            return new RoomListSearchDto
            {
                RoomId = result.room.RoomId,
                Title = result.room.Title,
                Description = result.room.Description,
                PricePerNight = result.room.PricePerNight ?? 0,
                MaxGuests = result.room.MaxGuests ?? 0,
                HostId = result.room.HostId ?? 0,
                HostName = result.user.Name,
                CityName = result.city.CityName,
                DistrictId = result.district.DistrictId,
                DistrictName = result.district.DistrictName,
                AddressLine = result.address.Street,
                Geo = (result.address.Latitude != null && result.address.Longitude != null)
                    ? new GeoLocation { Lat = (double)result.address.Latitude, Lng = (double)result.address.Longitude }
                    : null,
                CreatedAt = result.room.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = result.room.UpdatedAt ?? DateTime.MinValue,
                RatingAvg = 0,
                ReviewsCount = 0,
                CoverBucket = result.CoverPhoto?.Bucket,
                CoverObjectKey = result.CoverPhoto?.ObjectKey,
                CoverContentType = result.CoverPhoto?.ContentType,
                Amenities = new List<string>(),
                Status = result.room.Status,
                IsDeleted = result.room.IsDeleted
            };
        }

        private async Task<List<RoomListSearchDto>> GetRoomListDtosBatchAsync(int skip, int take)
        {
            var result = await (from room in _dbContext.RoomLists.Include(r => r.RoomPhotos)
                                join user in _dbContext.Users on room.HostId equals user.UserId
                                join address in _dbContext.Addresses on room.AddressId equals address.AddressId
                                join district in _dbContext.Districts on address.DistrictId equals district.DistrictId
                                join city in _dbContext.Cities on district.CityId equals city.CityId
                                orderby room.RoomId
                                select new
                                {
                                    room, user, address, district, city,
                                    CoverPhoto = room.RoomPhotos.OrderBy(p => p.SortOrder).FirstOrDefault(p => p.PhotoType == "Cover") ??
                                                 room.RoomPhotos.OrderBy(p => p.SortOrder).FirstOrDefault()
                                })
                                .Skip(skip)
                                .Take(take)
                                .ToListAsync();

            return result.Select(r => new RoomListSearchDto
            {
                RoomId = r.room.RoomId,
                Title = r.room.Title,
                Description = r.room.Description,
                PricePerNight = r.room.PricePerNight ?? 0,
                MaxGuests = r.room.MaxGuests ?? 0,
                HostId = r.room.HostId ?? 0,
                HostName = r.user.Name,
                CityName = r.city.CityName,
                DistrictId = r.district.DistrictId,
                DistrictName = r.district.DistrictName,
                AddressLine = r.address.Street,
                Geo = (r.address.Latitude != null && r.address.Longitude != null)
                    ? new GeoLocation { Lat = (double)r.address.Latitude, Lng = (double)r.address.Longitude }
                    : null,
                CreatedAt = r.room.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = r.room.UpdatedAt ?? DateTime.MinValue,
                RatingAvg = 0,
                ReviewsCount = 0,
                CoverBucket = r.CoverPhoto?.Bucket,
                CoverObjectKey = r.CoverPhoto?.ObjectKey,
                CoverContentType = r.CoverPhoto?.ContentType,
                Amenities = new List<string>(),
                Status = r.room.Status,
                IsDeleted = r.room.IsDeleted
            }).ToList();
        }

        public async Task<IEnumerable<RoomListSearchDto>> SearchNearbyAsync(
            string query,
            double lat,
            double lng,
            double radiusKm,
            string? status = null,
            bool sortByDistance = true)
        {
            _logger.LogInformation("=== MeilisearchService.SearchNearbyAsync 開始 ===");
            
            // 記錄 AI 搜尋狀態過濾變更
            if (string.IsNullOrWhiteSpace(status))
            {
                _logger.LogWarning("【AI搜尋模式】已移除狀態過濾，將返回所有未刪除的房源（各種狀態）");
            }
            
            _logger.LogInformation("輸入參數 - 查詢: '{Query}', 緯度: {Lat}, 經度: {Lng}, 半徑: {RadiusKm}km, 狀態: '{Status}', 距離排序: {SortByDistance}", 
                query, lat, lng, radiusKm, status, sortByDistance);
            
            var filters = new List<string>();
            // Always exclude deleted - 這是唯一的基本過濾條件
            filters.Add("is_deleted = false");
            
            // 狀態過濾：只有當明確提供狀態值時才添加過濾條件
            // 這允許 AI 搜尋顯示所有未刪除的房源，無論其狀態為何
            if (!string.IsNullOrWhiteSpace(status))
            {
                filters.Add($"status = \"{status}\"");
                _logger.LogInformation("添加狀態過濾條件: status = '{Status}'", status);
            }
            else
            {
                _logger.LogInformation("未提供狀態過濾，顯示所有未刪除房源 (is_deleted = false)");
            }

            var radiusMeters = radiusKm * 1000.0;
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            var geoFilter = $"_geoRadius({lat.ToString(inv)}, {lng.ToString(inv)}, {radiusMeters.ToString(inv)})";
            filters.Add(geoFilter);

            _logger.LogInformation("地理過濾器: {GeoFilter}", geoFilter);
            _logger.LogInformation("完整過濾條件: {Filters}", string.Join(" AND ", filters));

            try
            {
                _logger.LogInformation(
                    "Searching nearby: q='{Query}', lat={Lat}, lng={Lng}, radiusKm={RadiusKm}, status='{Status}'",
                    query, lat, lng, radiusKm, status);

                var index = _meiliClient.Index(IndexName);
                var searchQuery = new SearchQuery
                {
                    Q = query,
                    Filter = filters.Count > 0 ? string.Join(" AND ", filters) : null,
                    Limit = 200,
                };

                _logger.LogInformation("Meilisearch 查詢對象: {QueryObject}", System.Text.Json.JsonSerializer.Serialize(searchQuery));

                if (sortByDistance)
                {
                    searchQuery.Sort = new[] { $"_geoPoint({lat.ToString(inv)}, {lng.ToString(inv)}):asc" };
                    _logger.LogInformation("添加距離排序: {Sort}", searchQuery.Sort.First());
                }

                var searchResult = await index.SearchAsync<RoomListSearchDto>(query, searchQuery);
                var hits = searchResult.Hits.ToList();

                _logger.LogInformation("Meilisearch 返回 {Count} 個結果", hits.Count);

                // 添加更詳細的結果檢查
                if (hits.Any())
                {
                    _logger.LogInformation("找到房源，開始處理詳細信息");
                    var firstHit = hits.First();
                    _logger.LogInformation("第一個房源 - ID: {RoomId}, 標題: {Title}, 座標: ({Lat}, {Lng}), 狀態: {Status}, 是否刪除: {IsDeleted}", 
                        firstHit.RoomId, firstHit.Title, firstHit.Geo?.Lat, firstHit.Geo?.Lng, firstHit.Status, firstHit.IsDeleted);
                }
                else
                {
                    _logger.LogWarning("Meilisearch 沒有返回任何房源結果");
                    // 檢查是否有索引或地理數據問題
                    _logger.LogInformation("檢查可能的原因:");
                    _logger.LogInformation("1. 半徑設置: {RadiusKm}km ({RadiusMeters}m)", radiusKm, radiusMeters);
                    _logger.LogInformation("2. 中心點座標: ({Lat}, {Lng})", lat, lng);
                    _logger.LogInformation("3. 過濾條件: {Filters}", string.Join(" AND ", filters));
                    _logger.LogInformation("4. 查詢詞語: '{Query}'", query);
                }

                foreach (var hit in hits)
                {
                    if (!string.IsNullOrEmpty(hit.CoverObjectKey))
                    {
                        try
                        {
                            hit.CoverImageUrl = await _minioService.GetFileUrlAsync(hit.CoverObjectKey);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to get MinIO URL for object key '{ObjectKey}'.", hit.CoverObjectKey);
                            hit.CoverImageUrl = null;
                        }
                    }
                }

                if (hits.Any())
                {
                    var roomIds = hits.Select(h => h.RoomId)
                        .Where(id => id > 0)
                        .Distinct()
                        .ToList();

                    if (roomIds.Any())
                    {
                        var ratingStats = await _dbContext.Reviews
                            .Where(review => review.RoomId.HasValue && roomIds.Contains(review.RoomId.Value) && review.Rating.HasValue)
                            .GroupBy(review => review.RoomId!.Value)
                            .Select(group => new
                            {
                                RoomId = group.Key,
                                AverageRating = group.Average(review => review.Rating!.Value),
                                ReviewsCount = group.Count()
                            })
                            .ToDictionaryAsync(group => group.RoomId);

                        foreach (var hit in hits)
                        {
                            if (ratingStats.TryGetValue(hit.RoomId, out var stats))
                            {
                                hit.RatingAvg = stats.AverageRating;
                                hit.ReviewsCount = stats.ReviewsCount;
                            }
                            else
                            {
                                hit.RatingAvg = 0;
                                hit.ReviewsCount = 0;
                            }
                        }
                    }
                    else
                    {
                        foreach (var hit in hits)
                        {
                            hit.RatingAvg = 0;
                            hit.ReviewsCount = 0;
                        }
                    }
                }

                _logger.LogInformation("=== MeilisearchService.SearchNearbyAsync 完成，返回 {Count} 個房源 ===", hits.Count);
                return hits;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching nearby in Meilisearch.");
                return Enumerable.Empty<RoomListSearchDto>();
            }
        }
    }
}
