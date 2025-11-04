using RentalManagementPlatformWebAPI.DTO.RoomList;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Services.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetReviewsByRoomAsync(int roomId);
        Task<ReviewDto> CreateReviewAsync(CreateReviewDto createReviewDto, int reviewerId);
    }
}
