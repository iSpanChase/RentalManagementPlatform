using RentalManagementPlatformWebAPI.DTOs; // Changed to API's DTOs namespace
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Services.Interfaces
{
    public interface IRoomListQueryService
    {
        Task<List<RoomSummaryResponseDto>> GetRoomSummariesAsync(); // Changed to RoomSummaryResponseDto
        Task<RoomDetailsResponseDto?> GetRoomDetailsAsync(int id); // Changed to RoomDetailsResponseDto
        Task<RoomDetailsResponseDto?> GetRoomDataForIndexingAsync(int id); // Changed to RoomDetailsResponseDto
        Task<UpdateRoomRequestDto?> GetRoomForEditAsync(int id); // Changed to UpdateRoomRequestDto
        Task<RoomSummaryResponseDto?> GetRoomSummaryForDeleteAsync(int id); // Changed to RoomSummaryResponseDto
        Task<bool> RoomListExistsAsync(int id);
        Task<IEnumerable<RoomSummaryResponseDto>> GetHotRoomsAsync();
    }
}