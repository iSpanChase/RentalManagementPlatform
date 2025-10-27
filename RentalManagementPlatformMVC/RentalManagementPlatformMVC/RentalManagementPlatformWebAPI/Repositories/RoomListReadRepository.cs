using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;

namespace RentalManagementPlatformWebAPI.Repositories
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
}