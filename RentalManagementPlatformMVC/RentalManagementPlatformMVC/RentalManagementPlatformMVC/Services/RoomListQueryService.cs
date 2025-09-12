
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.Room_List.Models;
using RentalManagementPlatformMVC.Services.Interfaces;
using RentalManagementPlatformMVC.Repositories.Interfaces;
using RentalManagementPlatformMVC.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementPlatformMVC.Areas.Room_List.Services
{
    public class RoomListQueryService : IRoomListQueryService
    {
        private readonly IRoomListReadRepository _repository;
        private readonly MeilisearchService _meilisearchService;

        public RoomListQueryService(IRoomListReadRepository repository, MeilisearchService meilisearchService)
        {
            _repository = repository;
            _meilisearchService = meilisearchService;
        }

        public async Task<List<RoomSummaryViewModel>> GetRoomSummariesAsync()
        {
            var roomSummaries = await (from room in _repository.GetAll()
                                     join user in _repository.GetUsers() on room.HostId equals user.UserId
                                     select new RoomSummaryViewModel
                                     {
                                         RoomId = room.RoomId,
                                         Title = room.Title,
                                         Status = room.Status,
                                         HostName = user.Name
                                     })
                                     .ToListAsync();
            return roomSummaries;
        }

        public async Task<RoomDetailsViewModel?> GetRoomDetailsAsync(int id)
        {
            var roomDetails = await (from room in _repository.GetAll()
                                     where room.RoomId == id
                                     join user in _repository.GetUsers() on room.HostId equals user.UserId
                                     join address in _repository.GetAddresses() on room.AddressId equals address.AddressId
                                     join district in _repository.GetDistricts() on address.DistrictId equals district.DistrictId
                                     join city in _repository.GetCities() on district.CityId equals city.CityId
                                     select new RoomDetailsViewModel
                                     {
                                         RoomId = room.RoomId,
                                         Title = room.Title,
                                         Description = room.Description,
                                         MaxGuests = room.MaxGuests ?? 0,
                                         PricePerNight = room.PricePerNight ?? 0,
                                         Status = room.Status,
                                         Host = new HostViewModel { HostName = user.Name },
                                         HostId = user.UserId,
                                         CityName = city.CityName,
                                         DistrictId = district.DistrictId,
                                         DistrictName = district.DistrictName,
                                         Address = new AddressViewModel { FullAddress = city.CityName + district.DistrictName + address.Street },
                                         AddressLine = address.Street,
                                         Geo = new GeoLocation { Lat = (double)address.Latitude, Lng = (double)address.Longitude },
                                         RatingAvg = 0,
                                         ReviewsCount = 0,
                                         CoverBucket = null,
                                         CoverObjectKey = null,
                                         CoverContentType = null,
                                         CreatedAt = room.CreatedAt ?? System.DateTime.MinValue,
                                         UpdatedAt = room.UpdatedAt ?? System.DateTime.MinValue,
                                         PhotoUrls = new List<string>(),
                                         Amenities = new List<string>()
                                     }).FirstOrDefaultAsync();

            if (roomDetails != null)
            {
                var searchResult = await _meilisearchService.SearchAsync(roomDetails.RoomId.ToString());
                var roomFromMeilisearch = searchResult.FirstOrDefault();
                if (roomFromMeilisearch != null)
                {
                    roomDetails.Amenities = roomFromMeilisearch.Amenities;
                }
            }

            return roomDetails;
        }

        public async Task<RoomInputViewModel?> GetRoomForEditAsync(int id)
        {
            var roomList = await _repository.GetAll().FirstOrDefaultAsync(r => r.RoomId == id);
            if (roomList == null) return null;

            return new RoomInputViewModel
            {
                RoomId = roomList.RoomId,
                Title = roomList.Title,
                Description = roomList.Description,
                MaxGuests = roomList.MaxGuests ?? 0,
                PricePerNight = roomList.PricePerNight ?? 0
            };
        }

        public async Task<RoomSummaryViewModel?> GetRoomSummaryForDeleteAsync(int id)
        {
            return await (from room in _repository.GetAll()
                          where room.RoomId == id
                          join user in _repository.GetUsers() on room.HostId equals user.UserId
                          select new RoomSummaryViewModel
                          {
                              RoomId = room.RoomId,
                              Title = room.Title,
                              Status = room.Status,
                              HostName = user.Name
                          }).FirstOrDefaultAsync();
        }

        public async Task<bool> RoomListExistsAsync(int id)
        {
            return await _repository.GetAll().AnyAsync(e => e.RoomId == id);
        }
    }
}
