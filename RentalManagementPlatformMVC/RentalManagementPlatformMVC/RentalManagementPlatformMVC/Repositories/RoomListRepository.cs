
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformMVC.Repositories.Interfaces;
using RentalManagementPlatformMVC.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementPlatformMVC.Repositories
{
    public class RoomListReadRepository : IRoomListReadRepository
    {
        private readonly RentalManagementPlatformSqlContext _db;
        public RoomListReadRepository(RentalManagementPlatformSqlContext db) => _db = db;

        public IQueryable<RoomList> GetAll() => _db.RoomLists.AsNoTracking();
        public IQueryable<User> GetUsers() => _db.Users.AsNoTracking();
        public IQueryable<Address> GetAddresses() => _db.Addresses.AsNoTracking();
        public IQueryable<District> GetDistricts() => _db.Districts.AsNoTracking();
        public IQueryable<City> GetCities() => _db.Cities.AsNoTracking();
    }

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
