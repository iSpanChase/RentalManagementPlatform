using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.DTOs; // API's DTOs namespace
using RentalManagementPlatformWebAPI.Services.Interfaces; // API's Services.Interfaces namespace
using RentalManagementPlatformWebAPI.Repositories.Interfaces; // API's Repositories.Interfaces namespace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RentalManagementPlatformWebAPI.Models; // API's Models namespace
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using StackExchange.Redis;

namespace RentalManagementPlatformWebAPI.Services
{
    public class RoomListQueryService : IRoomListQueryService
    {
        private readonly IRoomListReadRepository _repository;
        private readonly MeilisearchService _meilisearchService; // Assuming MeilisearchService will be in API's Services
        private readonly IFileUrlResolver _urlResolver; // Assuming IFileUrlResolver will be in API's Services.Interfaces
        private readonly IDistributedCache _cache;

        public RoomListQueryService(IRoomListReadRepository repository, MeilisearchService meilisearchService, IFileUrlResolver urlResolver, IDistributedCache cache)
        {
            _repository = repository;
            _meilisearchService = meilisearchService;
            _urlResolver = urlResolver;
            _cache = cache;
        }

        public async Task<List<RoomSummaryResponseDto>> GetRoomSummariesAsync()
        {
            var roomsWithUsers = await (from room in _repository.GetAll().Include(r => r.RoomPhotos) // Include RoomPhotos here
                                        join user in _repository.GetUsers() on room.HostId equals user.UserId
                                        select new { Room = room, User = user })
                                        .ToListAsync();

            var roomSummaries = new List<RoomSummaryResponseDto>();

            foreach (var item in roomsWithUsers)
            {
                var summary = new RoomSummaryResponseDto
                {
                    RoomId = item.Room.RoomId,
                    Title = item.Room.Title,
                    Status = item.Room.Status, // Populating Status
                    HostName = item.User.Name,
                    IsDeleted = item.Room.IsDeleted // Populating IsDeleted
                };

                if (item.Room.RoomPhotos != null && item.Room.RoomPhotos.Any())
                {
                    var photoUrls = await _urlResolver.GetRoomPhotoUrlsAsync(item.Room.RoomId);
                    summary.MainImageUrl = photoUrls.FirstOrDefault();
                }
                roomSummaries.Add(summary);
            }

            return roomSummaries;
        }

        public async Task<RoomDetailsResponseDto?> GetRoomDetailsAsync(int id)
        {
            var rawData = await (from room in _repository.GetAll().Include(r => r.RoomPhotos)
                                     where room.RoomId == id
                                     join user in _repository.GetUsers() on room.HostId equals user.UserId
                                     join address in _repository.GetAddresses() on room.AddressId equals address.AddressId
                                     join district in _repository.GetDistricts() on address.DistrictId equals district.DistrictId
                                     join city in _repository.GetCities() on district.CityId equals city.CityId
                                     select new { room, user, address, district, city })
                                     .FirstOrDefaultAsync();

            if (rawData == null)
            {
                return null;
            }

            var photoTasks = rawData.room.RoomPhotos
                .OrderBy(p => p.SortOrder)
                .Select(async p => new PhotoDto
                {
                    PhotoId = p.PhotoId,
                    Url = await _urlResolver.GetPhotoUrlAsync(p) ?? string.Empty,
                    SortOrder = p.SortOrder
                });
            var photos = (await Task.WhenAll(photoTasks)).ToList();

            var mainPhoto = rawData.room.RoomPhotos?
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.PhotoId)
                .FirstOrDefault(p => p.PhotoType == "Cover");

            var (ratingAvg, reviewsCount) = await _repository.GetRoomRatingStatsAsync(id);

            var roomDetails = new RoomDetailsResponseDto
            {
                RoomId = rawData.room.RoomId,
                Title = rawData.room.Title,
                Description = rawData.room.Description,
                MaxGuests = rawData.room.MaxGuests ?? 0,
                PricePerNight = rawData.room.PricePerNight ?? 0,
                Status = rawData.room.Status,
                IsDeleted = rawData.room.IsDeleted,
                Host = new HostDto { HostName = rawData.user.Name },
                HostId = rawData.user.UserId,
                CityName = rawData.city.CityName,
                DistrictId = rawData.district.DistrictId,
                DistrictName = rawData.district.DistrictName,
                Address = new AddressDto { FullAddress = rawData.city.CityName + rawData.district.DistrictName + rawData.address.Street },
                AddressLine = rawData.address.Street,
                Geo = new GeoLocation { Lat = (double)rawData.address.Latitude, Lng = (double)rawData.address.Longitude },
                RatingAvg = ratingAvg ?? 0,
                ReviewsCount = reviewsCount,
                CoverBucket = mainPhoto?.Bucket,
                CoverObjectKey = mainPhoto?.ObjectKey,
                CoverContentType = mainPhoto?.ContentType,
                CreatedAt = rawData.room.CreatedAt ?? System.DateTime.MinValue,
                UpdatedAt = rawData.room.UpdatedAt ?? System.DateTime.MinValue,
                Photos = photos,
                Amenities = new List<string>()
            };

            var searchResult = await _meilisearchService.SearchAsync(roomDetails.RoomId.ToString());
            var roomFromMeilisearch = searchResult.FirstOrDefault();
            if (roomFromMeilisearch != null)
            {
                roomDetails.Amenities = roomFromMeilisearch.Amenities;
            }

            return roomDetails;
        }

        public async Task<RoomDetailsResponseDto?> GetRoomDataForIndexingAsync(int id)
        {
            var rawData = await (from room in _repository.GetAll().Include(r => r.RoomPhotos)
                                     where room.RoomId == id
                                     join user in _repository.GetUsers() on room.HostId equals user.UserId
                                     join address in _repository.GetAddresses() on room.AddressId equals address.AddressId
                                     join district in _repository.GetDistricts() on address.DistrictId equals district.DistrictId
                                     join city in _repository.GetCities() on district.CityId equals city.CityId
                                     select new { room, user, address, district, city })
                                     .FirstOrDefaultAsync();

            if (rawData == null)
            {
                return null;
            }

            var photoTasks = rawData.room.RoomPhotos
                .OrderBy(p => p.SortOrder)
                .Select(async p => new PhotoDto
                {
                    PhotoId = p.PhotoId,
                    Url = await _urlResolver.GetPhotoUrlAsync(p) ?? string.Empty,
                    SortOrder = p.SortOrder
                });
            var photos = (await Task.WhenAll(photoTasks)).ToList();

            var mainPhoto = rawData.room.RoomPhotos?
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.PhotoId)
                .FirstOrDefault(p => p.PhotoType == "Cover");

            var (ratingAvg, reviewsCount) = await _repository.GetRoomRatingStatsAsync(id);

            var roomDetails = new RoomDetailsResponseDto
            {
                RoomId = rawData.room.RoomId,
                Title = rawData.room.Title,
                Description = rawData.room.Description,
                MaxGuests = rawData.room.MaxGuests ?? 0,
                PricePerNight = rawData.room.PricePerNight ?? 0,
                Status = rawData.room.Status,
                IsDeleted = rawData.room.IsDeleted,
                Host = new HostDto { HostName = rawData.user.Name },
                HostId = rawData.user.UserId,
                CityName = rawData.city.CityName,
                DistrictId = rawData.district.DistrictId,
                DistrictName = rawData.district.DistrictName,
                Address = new AddressDto { FullAddress = rawData.city.CityName + rawData.district.DistrictName + rawData.address.Street },
                AddressLine = rawData.address.Street,
                Geo = new GeoLocation { Lat = (double)rawData.address.Latitude, Lng = (double)rawData.address.Longitude },
                RatingAvg = ratingAvg ?? 0,
                ReviewsCount = reviewsCount,
                CoverBucket = mainPhoto?.Bucket,
                CoverObjectKey = mainPhoto?.ObjectKey,
                CoverContentType = mainPhoto?.ContentType,
                CreatedAt = rawData.room.CreatedAt ?? System.DateTime.MinValue,
                UpdatedAt = rawData.room.UpdatedAt ?? System.DateTime.MinValue,
                Photos = photos,
                Amenities = new List<string>()
            };

            var searchResult = await _meilisearchService.SearchAsync(roomDetails.RoomId.ToString());
            var roomFromMeilisearch = searchResult.FirstOrDefault();
            if (roomFromMeilisearch != null)
            {
                roomDetails.Amenities = roomFromMeilisearch.Amenities;
            }

            return roomDetails;
        }

        public async Task<UpdateRoomRequestDto?> GetRoomForEditAsync(int id)
        {
            var roomList = await _repository.GetAll().FirstOrDefaultAsync(r => r.RoomId == id && r.IsDeleted == false);
            if (roomList == null) return null;

            return new UpdateRoomRequestDto
            {
                RoomId = roomList.RoomId,
                Title = roomList.Title,
                Description = roomList.Description,
                MaxGuests = roomList.MaxGuests ?? 0,
                PricePerNight = roomList.PricePerNight ?? 0
            };
        }

        public async Task<RoomSummaryResponseDto?> GetRoomSummaryForDeleteAsync(int id)
        {
            return await (from room in _repository.GetAll()
                          where room.RoomId == id
                          join user in _repository.GetUsers() on room.HostId equals user.UserId
                          select new RoomSummaryResponseDto
                          {
                              RoomId = room.RoomId,
                              Title = room.Title,
                              Status = room.Status,
                              HostName = user.Name
                          }).FirstOrDefaultAsync();
        }

        public async Task<bool> RoomListExistsAsync(int id)
        {
            return await _repository.GetAll().AnyAsync(e => e.RoomId == id && e.IsDeleted == false);
        }

        public async Task<IEnumerable<RoomSummaryResponseDto>> GetHotRoomsAsync()
        {
            const string cacheKey = "hot-rooms-random-selection";
            string? cachedData = null;

            try
            {
                cachedData = await _cache.GetStringAsync(cacheKey);
            }
            catch (RedisConnectionException)
            {
                // Ignore if Redis is down
            }

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<List<RoomSummaryResponseDto>>(cachedData) ?? new List<RoomSummaryResponseDto>();
            }
            else
            {
                const int numberOfRoomsToFetch = 10;
                var randomRoomsWithHost = await _repository.GetRandomRoomsWithHostAsync(numberOfRoomsToFetch);

                var roomSummaries = new List<RoomSummaryResponseDto>();
                foreach (var (room, host, ratingAvg, reviewsCount) in randomRoomsWithHost)
                {
                    var summary = new RoomSummaryResponseDto
                    {
                        RoomId = room.RoomId,
                        Title = room.Title,
                        Status = room.Status,
                        HostId = room.HostId ?? host.UserId,
                        HostName = host.Name,
                        Description = room.Description,
                        PricePerNight = room.PricePerNight ?? 0,
                        RatingAvg = ratingAvg ?? 0m,
                        ReviewsCount = reviewsCount,
                        CityName = room.Address?.District?.City?.CityName,
                        DistrictName = room.Address?.District?.DistrictName,
                        AddressLine = room.Address?.Street,
                        IsDeleted = room.IsDeleted,
                        MaxGuests = room.MaxGuests ?? 0,
                        Geo = room.Address != null
                            ? new GeoLocation
                            {
                                Lat = (double)room.Address.Latitude,
                                Lng = (double)room.Address.Longitude
                            }
                            : null
                    };

                    var photoUrls = await _urlResolver.GetRoomPhotoUrlsAsync(room.RoomId);
                    summary.PhotoUrls = photoUrls.ToList();
                    if (string.IsNullOrEmpty(summary.MainImageUrl))
                    {
                        summary.MainImageUrl = summary.PhotoUrls.FirstOrDefault();
                    }

                    roomSummaries.Add(summary);
                }

                var serializedRooms = JsonSerializer.Serialize(roomSummaries);
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                try
                {
                    await _cache.SetStringAsync(cacheKey, serializedRooms, cacheOptions);
                }
                catch (RedisConnectionException)
                {
                    // Ignore
                }

                return roomSummaries;
            }
        }
    }
}
