
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformMVC.Repositories.Interfaces;
using RentalManagementPlatformMVC.Data;
using System.Linq;
using System.Threading.Tasks;
using System;

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

        public async Task<(double? RatingAvg, int ReviewsCount)> GetRoomRatingStatsAsync(int roomId)
        {
            var stats = await _db.Reviews
                .Where(review => review.RoomId == roomId && review.Rating.HasValue)
                .GroupBy(review => review.RoomId!.Value)
                .Select(group => new
                {
                    AverageRating = group.Average(review => (double)review.Rating!.Value),
                    ReviewsCount = group.Count()
                })
                .FirstOrDefaultAsync();

            if (stats == null)
            {
                return (null, 0);
            }

            var roundedAverage = Math.Round(stats.AverageRating, 1, MidpointRounding.AwayFromZero);
            return (roundedAverage, stats.ReviewsCount);
        }
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
