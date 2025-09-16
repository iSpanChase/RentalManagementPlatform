
using Meilisearch;
using RentalManagementPlatformMVC.Areas.Room_List.Models;
using RentalManagementPlatformMVC.Services.Interfaces;
using RentalManagementPlatformMVC.Repositories.Interfaces;
using RentalManagementPlatformMVC.Models;
using System;
using System.Threading.Tasks;

namespace RentalManagementPlatformMVC.Areas.Room_List.Services
{
    public class RoomListCommandService : IRoomListCommandService
    {
        private readonly IRoomListWriteRepository _writeRepository;
        private readonly IRoomListQueryService _queryService; // To get DTO for Meilisearch
        private readonly MeilisearchClient _meilisearchClient;

        public RoomListCommandService(IRoomListWriteRepository writeRepository, IRoomListQueryService queryService, MeilisearchClient meilisearchClient)
        {
            _writeRepository = writeRepository;
            _queryService = queryService;
            _meilisearchClient = meilisearchClient;
        }

        public async Task<RoomList> CreateRoomAsync(RoomInputViewModel viewModel)
        {
            var roomList = new RoomList
            {
                Title = viewModel.Title,
                Description = viewModel.Description,
                MaxGuests = viewModel.MaxGuests,
                PricePerNight = viewModel.PricePerNight,
                Status = "Available",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _writeRepository.AddAsync(roomList);
            await _writeRepository.SaveChangesAsync();

            await UpdateSearchIndexAsync(roomList.RoomId);

            return roomList;
        }

        public async Task UpdateRoomAsync(int id, RoomInputViewModel viewModel)
        {
            var roomList = await _writeRepository.FindAsync(id);
            if (roomList == null)
            {
                // Or throw a custom not found exception
                return;
            }

            roomList.Title = viewModel.Title;
            roomList.Description = viewModel.Description;
            roomList.MaxGuests = viewModel.MaxGuests;
            roomList.PricePerNight = viewModel.PricePerNight;
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
            // This re-uses the logic from the query service to get the full DTO
            // In a real-world scenario, you might have a dedicated DTO builder for this
            var roomDetails = await _queryService.GetRoomDataForIndexingAsync(roomId);

            if (roomDetails != null)
            {
                // Map RoomDetailsViewModel to RoomListSearchDto if they are different
                // For now, assuming they are compatible or a similar DTO can be constructed
                var roomSearchDto = new RoomListSearchDto
                {
                    RoomId = roomDetails.RoomId,
                    Title = roomDetails.Title,
                    Description = roomDetails.Description,
                    PricePerNight = roomDetails.PricePerNight,
                    MaxGuests = roomDetails.MaxGuests,
                    HostId = roomDetails.HostId,
                    CityName = roomDetails.CityName,
                    DistrictId = roomDetails.DistrictId,
                    DistrictName = roomDetails.DistrictName,
                    AddressLine = roomDetails.AddressLine,
                    Geo = roomDetails.Geo,
                    CreatedAt = roomDetails.CreatedAt,
                    UpdatedAt = roomDetails.UpdatedAt,
                    Status = roomDetails.Status,
                    IsDeleted = roomDetails.IsDeleted,
                    Amenities = roomDetails.Amenities
                };

                var index = _meilisearchClient.Index("rooms");
                await index.AddDocumentsAsync(new[] { roomSearchDto });
            }
        }
    }
}
