using RentalManagementPlatformWebAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Repositories.Interfaces
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetReviewsByRoomIdAsync(int roomId);
        Task<Review> AddReviewAsync(Review review);
    }
}
