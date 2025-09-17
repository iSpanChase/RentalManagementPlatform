
using RentalManagementPlatformMVC.Areas.Room_List.Models;
using RentalManagementPlatformMVC.Models;
using System.Threading.Tasks;

namespace RentalManagementPlatformMVC.Services.Interfaces
{
    public interface IRoomListCommandService
    {
        Task<RoomList> CreateRoomAsync(RoomInputViewModel viewModel);
        Task UpdateRoomAsync(int id, RoomInputViewModel viewModel);
        Task DeleteRoomAsync(int id);
    }
}
