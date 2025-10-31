using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories.Interfaces
{
	public interface IRoomRepository
	{
		Task<RoomList?> GetByIdAsync(int roomId);
	}
}
