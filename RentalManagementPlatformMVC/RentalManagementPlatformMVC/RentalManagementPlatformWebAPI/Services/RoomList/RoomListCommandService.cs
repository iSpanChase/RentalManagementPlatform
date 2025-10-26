using Meilisearch;
using RentalManagementPlatformWebAPI.DTOs; // API's DTOs namespace
using RentalManagementPlatformWebAPI.Services.Interfaces; // API's Services.Interfaces namespace
using RentalManagementPlatformWebAPI.Repositories.Interfaces; // API's Repositories.Interfaces namespace
using RentalManagementPlatformWebAPI.Models; // API's Models namespace
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Services
{
    public class RoomListCommandService : IRoomListCommandService
    {
        private readonly IRoomListWriteRepository _writeRepository;
        private readonly IRoomListQueryService _queryService;
        private readonly MeilisearchClient _meilisearchClient;

        public RoomListCommandService(IRoomListWriteRepository writeRepository, IRoomListQueryService queryService, MeilisearchClient meilisearchClient)
        {
            _writeRepository = writeRepository;
            _queryService = queryService;
            _meilisearchClient = meilisearchClient;
        }

        public async Task<RoomList> CreateRoomAsync(CreateRoomRequestDto dto)
        {
            var roomList = new RoomList
            {
                Title = dto.Title,
                Description = dto.Description,
                MaxGuests = dto.MaxGuests,
                PricePerNight = dto.PricePerNight,
                Status = "Available",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _writeRepository.AddAsync(roomList);
            await _writeRepository.SaveChangesAsync();

            await UpdateSearchIndexAsync(roomList.RoomId);

            return roomList;
        }

        public async Task UpdateRoomAsync(int id, UpdateRoomRequestDto dto)
        {
            var roomList = await _writeRepository.FindAsync(id);
            if (roomList == null)
            {
                // Or throw a custom not found exception
                return;
            }

            roomList.Title = dto.Title;
            roomList.Description = dto.Description;
            roomList.MaxGuests = dto.MaxGuests;
            roomList.PricePerNight = dto.PricePerNight;
            roomList.UpdatedAt = DateTime.UtcNow;

            _writeRepository.Update(roomList);
            await _writeRepository.SaveChangesAsync();

            await UpdateSearchIndexAsync(id);
        }
        public async Task DeleteRoomAsync(int id)
        {
            var roomList = await _writeRepository.FindAsync(id);
            if (roomList != null)
            {
                roomList.IsDeleted = true;
                roomList.Status = "已刪除";
                roomList.UpdatedAt = DateTime.UtcNow;
                _writeRepository.Update(roomList);
                await _writeRepository.SaveChangesAsync();

                await UpdateSearchIndexAsync(id);
            }
        }

        private async Task UpdateSearchIndexAsync(int roomId)
        {
            var roomDetails = await _queryService.GetRoomDataForIndexingAsync(roomId);

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

                var index = _meilisearchClient.Index("rooms");
                await index.AddDocumentsAsync(new[] { roomSearchDto });
            }
        }
    }
}