using RentalManagementPlatformWebAPI.Models; // Changed to API's Models namespace
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Repositories.Interfaces
{
    public interface IRoomListWriteRepository
    {
        Task AddAsync(RoomList entity);
        void Update(RoomList entity);
        void Remove(RoomList entity);
        Task<RoomList?> FindAsync(int id);
        Task<int> SaveChangesAsync();
    }
}