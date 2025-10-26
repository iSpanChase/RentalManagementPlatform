using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.DTOs; // API's DTOs namespace
using RentalManagementPlatformWebAPI.Services.Interfaces; // API's Services.Interfaces namespace
using RentalManagementPlatformWebAPI.Repositories.Interfaces; // API's Repositories.Interfaces namespace
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RentalManagementPlatformWebAPI.Models; // API's Models namespace

namespace RentalManagementPlatformWebAPI.Services
{
    public class RoomListQueryService : IRoomListQueryService
    {
        private readonly IRoomListReadRepository _repository;
        private readonly MeilisearchService _meilisearchService; // Assuming MeilisearchService will be in API's Services
        private readonly IFileUrlResolver _urlResolver; // Assuming IFileUrlResolver will be in API's Services.Interfaces

        public RoomListQueryService(IRoomListReadRepository repository, MeilisearchService meilisearchService, IFileUrlResolver urlResolver)
        {
            _repository = repository;
            _meilisearchService = meilisearchService;
            _urlResolver = urlResolver;
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
                    var mainPhoto = item.Room.RoomPhotos.OrderBy(p => p.SortOrder).FirstOrDefault(p => p.PhotoType == "Cover") ?? item.Room.RoomPhotos.OrderBy(p => p.SortOrder).FirstOrDefault();
                    if (mainPhoto != null)
                    {
                        summary.MainImageUrl = await _urlResolver.GetUrlAsync("Room", item.Room.RoomId, mainPhoto.PhotoType);
                    }
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

            var photoUrls = new List<string>();
            if (rawData.room.RoomPhotos != null)
            {
                foreach (var photo in rawData.room.RoomPhotos.OrderBy(p => p.SortOrder))
                {
                    var url = await _urlResolver.GetUrlAsync("Room", rawData.room.RoomId, photo.PhotoType);
                    if (!string.IsNullOrEmpty(url))
                    {
                        photoUrls.Add(url);
                    }
                }
            }

            var mainPhoto = rawData.room.RoomPhotos?.OrderBy(p => p.SortOrder).FirstOrDefault(p => p.PhotoType == "Cover") ?? rawData.room.RoomPhotos?.OrderBy(p => p.SortOrder).FirstOrDefault();

            var roomDetails = new RoomDetailsResponseDto
            {
                RoomId = rawData.room.RoomId,
                Title = rawData.room.Title,
                Description = rawData.room.Description,
                MaxGuests = rawData.room.MaxGuests ?? 0,
                PricePerNight = rawData.room.PricePerNight ?? 0,
                Status = rawData.room.Status,
                IsDeleted = rawData.room.IsDeleted,
                Host = new HostDto { HostName = rawData.user.Name }, // Changed to HostDto
                HostId = rawData.user.UserId,
                CityName = rawData.city.CityName,
                DistrictId = rawData.district.DistrictId,
                DistrictName = rawData.district.DistrictName,
                Address = new AddressDto { FullAddress = rawData.city.CityName + rawData.district.DistrictName + rawData.address.Street }, // Changed to AddressDto
                AddressLine = rawData.address.Street,
                Geo = new GeoLocation { Lat = (double)rawData.address.Latitude, Lng = (double)rawData.address.Longitude },
                RatingAvg = 0,
                ReviewsCount = 0,
                CoverBucket = mainPhoto?.Bucket,
                CoverObjectKey = mainPhoto?.ObjectKey,
                CoverContentType = mainPhoto?.ContentType,
                CreatedAt = rawData.room.CreatedAt ?? System.DateTime.MinValue,
                UpdatedAt = rawData.room.UpdatedAt ?? System.DateTime.MinValue,
                PhotoUrls = photoUrls,
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
            
            var photoUrls = new List<string>();
            if (rawData.room.RoomPhotos != null)
            {
                foreach (var photo in rawData.room.RoomPhotos.OrderBy(p => p.SortOrder))
                {
                    var url = await _urlResolver.GetUrlAsync("Room", rawData.room.RoomId, photo.PhotoType);
                    if (!string.IsNullOrEmpty(url))
                    {
                        photoUrls.Add(url);
                    }
                }
            }

            var mainPhoto = rawData.room.RoomPhotos?.OrderBy(p => p.SortOrder).FirstOrDefault(p => p.PhotoType == "Cover") ?? rawData.room.RoomPhotos?.OrderBy(p => p.SortOrder).FirstOrDefault();

            var roomDetails = new RoomDetailsResponseDto
            {
                RoomId = rawData.room.RoomId,
                Title = rawData.room.Title,
                Description = rawData.room.Description,
                MaxGuests = rawData.room.MaxGuests ?? 0,
                PricePerNight = rawData.room.PricePerNight ?? 0,
                Status = rawData.room.Status,
                IsDeleted = rawData.room.IsDeleted,
                Host = new HostDto { HostName = rawData.user.Name }, // Changed to HostDto
                HostId = rawData.user.UserId,
                CityName = rawData.city.CityName,
                DistrictId = rawData.district.DistrictId,
                DistrictName = rawData.district.DistrictName,
                Address = new AddressDto { FullAddress = rawData.city.CityName + rawData.district.DistrictName + rawData.address.Street }, // Changed to AddressDto
                AddressLine = rawData.address.Street,
                Geo = new GeoLocation { Lat = (double)rawData.address.Latitude, Lng = (double)rawData.address.Longitude },
                RatingAvg = 0,
                ReviewsCount = 0,
                CoverBucket = mainPhoto?.Bucket,
                CoverObjectKey = mainPhoto?.ObjectKey,
                CoverContentType = mainPhoto?.ContentType,
                CreatedAt = rawData.room.CreatedAt ?? System.DateTime.MinValue,
                UpdatedAt = rawData.room.UpdatedAt ?? System.DateTime.MinValue,
                PhotoUrls = photoUrls,
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
    }
}
