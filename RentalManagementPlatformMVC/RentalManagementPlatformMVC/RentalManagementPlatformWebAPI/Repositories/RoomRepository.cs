using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;

namespace RentalManagementPlatformWebAPI.Repositories
{
	public class RoomRepository : IRoomRepository
	{
		private readonly RentalManagementPlatformSqlContext _context;

		public RoomRepository(RentalManagementPlatformSqlContext context)
		{
			_context = context;
		}

		public async Task<RoomList?> GetByIdAsync(int roomId)
		{
			return await _context.RoomLists
				.FirstOrDefaultAsync(r => r.RoomId == roomId);
		}
	}
}
