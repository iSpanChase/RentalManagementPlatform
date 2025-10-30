using RentalManagementPlatformWebAPI.Models; 
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using RentalManagementPlatformWebAPI.DTOs; // Correct DTO namespace

namespace RentalManagementPlatformWebAPI.Repositories.Interfaces
{
    public interface IRoomListReadRepository
    {
        IQueryable<RoomList> GetAll();
        IQueryable<User> GetUsers();
        IQueryable<Address> GetAddresses();
        IQueryable<District> GetDistricts();
        IQueryable<City> GetCities();
        Task<IEnumerable<(RoomList Room, User Host, decimal? RatingAvg, int ReviewsCount)>> GetRandomRoomsWithHostAsync(int count);
        Task<(double? RatingAvg, int ReviewsCount)> GetRoomRatingStatsAsync(int roomId);
    }
}
