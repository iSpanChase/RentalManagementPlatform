
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

        public MeilisearchService(
            MeilisearchClient meiliClient,
            RentalManagementPlatformSqlContext dbContext,
            ILogger<MeilisearchService> logger,
            IMinioService minioService)
        {
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
                var searchResult = await index.SearchAsync<RoomListSearchDto>(query, new SearchQuery { Limit = 20 });

                _logger.LogInformation("Meilisearch raw response: {RawResponse}", System.Text.Json.JsonSerializer.Serialize(searchResult));

                var hits = searchResult.Hits.ToList(); // Use ToList() to allow modification
                _logger.LogInformation("Meilisearch returned {Count} hits.", hits.Count);

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
            var result = await (from room in _dbContext.RoomLists
                                where room.RoomId == roomId
                                join user in _dbContext.Users on room.HostId equals user.UserId
                                join address in _dbContext.Addresses on room.AddressId equals address.AddressId
                                join district in _dbContext.Districts on address.DistrictId equals district.DistrictId
                                join city in _dbContext.Cities on district.CityId equals city.CityId
                                select new RoomListSearchDto
                                {
                                    RoomId = room.RoomId,
                                    Title = room.Title,
                                    Description = room.Description,
                                    PricePerNight = room.PricePerNight ?? 0,
                                    MaxGuests = room.MaxGuests ?? 0,
                                    HostId = room.HostId ?? 0,
                                    HostName = user.Name,
                                    CityName = city.CityName,
                                    DistrictId = district.DistrictId,
                                    DistrictName = district.DistrictName,
                                    AddressLine = address.Street,
                                    Geo = new GeoLocation { Lat = (double)address.Latitude, Lng = (double)address.Longitude },
                                    CreatedAt = room.CreatedAt ?? DateTime.MinValue,
                                    UpdatedAt = room.UpdatedAt ?? DateTime.MinValue,
                                    // The following fields are placeholders.
                                    // You need to implement the logic to populate them, likely with more joins.
                                    RatingAvg = 0,
                                    ReviewsCount = 0,
                                    CoverBucket = "",
                                    CoverObjectKey = "",
                                    CoverContentType = "",
                                    Amenities = new List<string>()
                                }).FirstOrDefaultAsync();
            return result;
        }

        private async Task<List<RoomListSearchDto>> MapAllToDto()
        {
            var result = await (from room in _dbContext.RoomLists
                                join user in _dbContext.Users on room.HostId equals user.UserId
                                join address in _dbContext.Addresses on room.AddressId equals address.AddressId
                                join district in _dbContext.Districts on address.DistrictId equals district.DistrictId
                                join city in _dbContext.Cities on district.CityId equals city.CityId
                                select new RoomListSearchDto
                                {
                                    RoomId = room.RoomId,
                                    Title = room.Title,
                                    Description = room.Description,
                                    PricePerNight = room.PricePerNight ?? 0,
                                    MaxGuests = room.MaxGuests ?? 0,
                                    HostId = room.HostId ?? 0,
                                    HostName = user.Name,
                                    CityName = city.CityName,
                                    DistrictId = district.DistrictId,
                                    DistrictName = district.DistrictName,
                                    AddressLine = address.Street,
                                    Geo = new GeoLocation { Lat = (double)address.Latitude, Lng = (double)address.Longitude },
                                    CreatedAt = room.CreatedAt ?? DateTime.MinValue,
                                    UpdatedAt = room.UpdatedAt ?? DateTime.MinValue,
                                    // The following fields are placeholders.
                                    // You need to implement the logic to populate them, likely with more joins.
                                    RatingAvg = 0,
                                    ReviewsCount = 0,
                                    CoverBucket = "",
                                    CoverObjectKey = "",
                                    CoverContentType = "",
                                    Amenities = new List<string>()
                                }).ToListAsync();
            return result;
        }
    }
}
