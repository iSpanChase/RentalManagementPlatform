
using RentalManagementPlatformMVC.Models;
using System.Linq;

namespace RentalManagementPlatformMVC.Repositories.Interfaces
{
    public interface IRoomListReadRepository
    {
        IQueryable<RoomList> GetAll();
        IQueryable<User> GetUsers();
        IQueryable<Address> GetAddresses();
        IQueryable<District> GetDistricts();
        IQueryable<City> GetCities();
    }
}
