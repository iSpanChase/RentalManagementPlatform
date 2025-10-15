using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformWebAPI.Repositories.Interface
{
	public interface IRoomRepository
	{
		Task<RoomList?> GetByIdAsync(int roomId);
	}
}
