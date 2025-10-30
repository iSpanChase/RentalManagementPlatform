using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;
using RentalManagementPlatformWebAPI.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<IEnumerable<(RoomList Room, User Host, decimal? RatingAvg, int ReviewsCount)>> GetRandomRoomsWithHostAsync(int count)
        {
            var randomRooms = await _db.RoomLists
                .Where(r => r.IsDeleted == false && (r.Status == null || r.Status != "已刪除"))
                .OrderBy(r => Guid.NewGuid()) // This translates to NEWID() in SQL Server
                .Take(count)
                .Include(r => r.RoomPhotos)
                .Include(r => r.Address)
                    .ThenInclude(a => a.District)
                        .ThenInclude(d => d.City)
                .ToListAsync();

            if (!randomRooms.Any())
            {
                return Enumerable.Empty<(RoomList, User, decimal?, int)>();
            }

            var hostIds = randomRooms.Select(r => r.HostId).Distinct();
            var hosts = await _db.Users.Where(u => hostIds.Contains(u.UserId)).ToDictionaryAsync(u => u.UserId);

            var results = randomRooms.Select(room => (
                Room: room,
                Host: hosts.TryGetValue((int)room.HostId, out var host) ? host : null
            )).Where(r => r.Host != null);

            var roomHostPairs = results.Select(r => (Room: r.Room, Host: r.Host!)).ToList();

            var roomIds = roomHostPairs.Select(r => r.Room.RoomId).ToList();

            var ratingStats = await _db.Reviews
                .Where(review => review.RoomId.HasValue && roomIds.Contains(review.RoomId.Value) && review.Rating.HasValue)
                .GroupBy(review => review.RoomId!.Value)
                .Select(group => new
                {
                    RoomId = group.Key,
                    AverageRating = group.Average(review => (decimal)review.Rating!),
                    ReviewsCount = group.Count()
                })
                .ToDictionaryAsync(group => group.RoomId);

            return roomHostPairs.Select(tuple =>
            {
                if (ratingStats.TryGetValue(tuple.Room.RoomId, out var stats))
                {
                    var roundedAverage = Math.Round(stats.AverageRating, 1, MidpointRounding.AwayFromZero);
                    return (tuple.Room, tuple.Host, (decimal?)roundedAverage, stats.ReviewsCount);
                }

                return (tuple.Room, tuple.Host, (decimal?)null, 0);
            });
        }

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
}
