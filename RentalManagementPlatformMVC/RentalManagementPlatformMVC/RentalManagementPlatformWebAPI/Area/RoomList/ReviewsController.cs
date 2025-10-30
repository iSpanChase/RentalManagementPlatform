using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTO.RoomList;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Area.RoomList.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("room/{roomId}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsByRoom(int roomId)
        {
            var reviews = await _reviewService.GetReviewsByRoomAsync(roomId);
            return Ok(reviews);
        }

        [HttpPost]
        [Authorize] // Assuming only authenticated users can create reviews
        public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] CreateReviewDto createReviewDto)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var newReview = await _reviewService.CreateReviewAsync(createReviewDto, userId);
            return CreatedAtAction(nameof(GetReviewsByRoom), new { roomId = createReviewDto.RoomId }, newReview);
        }
    }
}
