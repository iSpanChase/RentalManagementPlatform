
using RentalManagementPlatformMVC.Models;
using System.Threading.Tasks;

namespace RentalManagementPlatformMVC.Repositories.Interfaces
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
