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
    }
}
