
using RentalManagementPlatformMVC.Models;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementPlatformMVC.Repositories.Interfaces
{
    public interface IRoomListReadRepository
    {
        IQueryable<RoomList> GetAll();
        IQueryable<User> GetUsers();
        IQueryable<Address> GetAddresses();
        IQueryable<District> GetDistricts();
        IQueryable<City> GetCities();
        Task<(double? RatingAvg, int ReviewsCount)> GetRoomRatingStatsAsync(int roomId);
    }
}
