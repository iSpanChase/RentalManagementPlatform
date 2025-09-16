
using Meilisearch;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Data;
using RentalManagementPlatformMVC.Areas.Room_List.Models;
using RentalManagementPlatformMVC.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Text.Json;

namespace RentalManagementPlatformMVC.Services
{
    public class MeilisearchService
    {
        private readonly MeilisearchClient _meiliClient;
        private readonly RentalManagementPlatformSqlContext _dbContext;
        private readonly ILogger<MeilisearchService> _logger;
        private readonly IMinioService _minioService;
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

        public async Task<IEnumerable<RoomListSearchDto>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Enumerable.Empty<RoomListSearchDto>();
            }

            try
            {
                _logger.LogInformation("Searching Meilisearch index '{IndexName}' with query: '{Query}'.", IndexName, query);
                var index = _meiliClient.Index(IndexName);
                var searchResult = await index.SearchAsync<RoomListSearchDto>(query, new SearchQuery { Limit = 200 });

                _logger.LogInformation("Meilisearch raw response: {RawResponse}", System.Text.Json.JsonSerializer.Serialize(searchResult));

                var hits = searchResult.Hits.ToList(); // Use ToList() to allow modification
                _logger.LogInformation("Meilisearch returned {Count} hits.", hits.Count);

                foreach (var hit in hits)
                {
                    _logger.LogInformation("DEBUG: RoomId={RoomId}, Title='{Title}', IsDeleted={IsDeleted}", hit.RoomId, hit.Title, hit.IsDeleted);
                }

                // Populate CoverImageUrl from MinIO for each hit
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
                            hit.CoverImageUrl = null; // Ensure it's null on failure
                        }
                    }
                }

                if (hits.Any())
                {
                    var firstHit = hits.First();
                    _logger.LogInformation("First hit details: RoomId={RoomId}, Title='{Title}', CoverImageUrl='{CoverImageUrl}'",
                        firstHit.RoomId, firstHit.Title, firstHit.CoverImageUrl);
                }
                return hits;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching Meilisearch index '{IndexName}'.", IndexName);
                return Enumerable.Empty<RoomListSearchDto>();
            }
        }

        public async Task IndexAllRoomListsAsync()
        {
            try
            {
                _logger.LogInformation("Starting to index all room lists.");
                var dtos = await MapAllToDto();

                if (dtos.Any())
                {
                    var index = _meiliClient.Index(IndexName);
                    // Use RoomId as the primary key for Meilisearch
                    var taskInfo = await index.AddDocumentsAsync(dtos, "RoomId");
                    _logger.LogInformation("Successfully sent {Count} documents to Meilisearch. Task ID: {TaskId}", dtos.Count, taskInfo.TaskUid);
                }
                else
                {
                    _logger.LogInformation("No room lists found to index.");
                }
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
                await index.AddDocumentsAsync(new[] { dto }, "RoomId");
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
                var task =                 await index.DeleteOneDocumentAsync(roomId.ToString());
                _logger.LogInformation("Successfully requested deletion for document Room ID {RoomId}. Task ID: {TaskId}", roomId, task.TaskUid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete document for Room ID {RoomId}.", roomId);
            }
        }

        private async Task<RoomListSearchDto?> MapSingleToDto(int roomId)
        {
            var result = await (from room in _dbContext.RoomLists.Include(r => r.RoomPhotos) // Include RoomPhotos
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
                Geo = new GeoLocation { Lat = (double)result.address.Latitude, Lng = (double)result.address.Longitude },
                CreatedAt = result.room.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = result.room.UpdatedAt ?? DateTime.MinValue,
                RatingAvg = 0, // Placeholder
                ReviewsCount = 0, // Placeholder
                CoverBucket = result.CoverPhoto?.Bucket, // Populated
                CoverObjectKey = result.CoverPhoto?.ObjectKey, // Populated
                CoverContentType = result.CoverPhoto?.ContentType, // Populated
                Amenities = new List<string>(), // Populated elsewhere
                Status = result.room.Status,
                IsDeleted = result.room.IsDeleted
            };
        }

        private async Task<List<RoomListSearchDto>> MapAllToDto()
        {
            var result = await (from room in _dbContext.RoomLists.Include(r => r.RoomPhotos) // Include RoomPhotos
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
                Geo = new GeoLocation { Lat = (double)r.address.Latitude, Lng = (double)r.address.Longitude },
                CreatedAt = r.room.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = r.room.UpdatedAt ?? DateTime.MinValue,
                RatingAvg = 0, // Placeholder
                ReviewsCount = 0, // Placeholder
                CoverBucket = r.CoverPhoto?.Bucket, // Populated
                CoverObjectKey = r.CoverPhoto?.ObjectKey, // Populated
                CoverContentType = r.CoverPhoto?.ContentType, // Populated
                Amenities = new List<string>(), // Populated elsewhere
                Status = r.room.Status,
                IsDeleted = r.room.IsDeleted
            }).ToList();
        }
    }
}
