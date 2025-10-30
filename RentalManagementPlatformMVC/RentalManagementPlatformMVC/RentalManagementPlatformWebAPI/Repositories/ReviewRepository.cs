using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly RentalManagementPlatformSqlContext _context;

        public ReviewRepository(RentalManagementPlatformSqlContext context)
        {
            _context = context;
        }

        public async Task<Review> AddReviewAsync(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<IEnumerable<Review>> GetReviewsByRoomIdAsync(int roomId)
        {            
            return await _context.Reviews
                .Where(r => r.RoomId == roomId)
                .ToListAsync();
        }
    }
}
