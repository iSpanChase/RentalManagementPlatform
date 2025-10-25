using RentalManagementPlatformWebAPI.Models; // Changed to API's Models namespace
using System.Linq;

namespace RentalManagementPlatformWebAPI.Repositories.Interfaces
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