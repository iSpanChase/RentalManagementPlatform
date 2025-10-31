using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Repositories
{
    public class RoomListWriteRepository : IRoomListWriteRepository
    {
        private readonly RentalManagementPlatformSqlContext _db;
        public RoomListWriteRepository(RentalManagementPlatformSqlContext db) => _db = db;

        public async Task AddAsync(RoomList entity)
        {
            await _db.RoomLists.AddAsync(entity);
        }

        public void Update(RoomList entity)
        {
            _db.RoomLists.Update(entity);
        }

        public void Remove(RoomList entity)
        {
            _db.RoomLists.Remove(entity);
        }

        public async Task<RoomList?> FindAsync(int id)
        {
            return await _db.RoomLists.FindAsync(id);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync();
        }
    }
}