using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories.Interface
{
	public interface IRoomRepository
	{
		Task<RoomList?> GetByIdAsync(int roomId);
	}
}
