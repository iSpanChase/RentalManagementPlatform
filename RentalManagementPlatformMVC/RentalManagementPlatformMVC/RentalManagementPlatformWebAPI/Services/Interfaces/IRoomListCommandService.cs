using RentalManagementPlatformWebAPI.DTOs; // Changed to API's DTOs namespace
using RentalManagementPlatformWebAPI.Models; // Changed to API's Models namespace
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Services.Interfaces
{
    public interface IRoomListCommandService
    {
        Task<RoomList> CreateRoomAsync(CreateRoomRequestDto dto);
        Task UpdateRoomAsync(int id, UpdateRoomRequestDto dto);
        Task DeleteRoomAsync(int id);
    }
}