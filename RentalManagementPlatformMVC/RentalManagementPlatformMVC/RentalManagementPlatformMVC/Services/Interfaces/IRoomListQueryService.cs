
using RentalManagementPlatformMVC.Areas.Room_List.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RentalManagementPlatformMVC.Services.Interfaces
{
    public interface IRoomListQueryService
    {
        Task<List<RoomSummaryViewModel>> GetRoomSummariesAsync();
        Task<RoomDetailsViewModel?> GetRoomDetailsAsync(int id);
        Task<RoomDetailsViewModel?> GetRoomDataForIndexingAsync(int id);
        Task<RoomInputViewModel?> GetRoomForEditAsync(int id);
        Task<RoomSummaryViewModel?> GetRoomSummaryForDeleteAsync(int id);
        Task<bool> RoomListExistsAsync(int id);
    }
}
