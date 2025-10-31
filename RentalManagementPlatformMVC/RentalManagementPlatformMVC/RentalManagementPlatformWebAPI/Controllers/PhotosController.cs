using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IRoomListCommandService _commandService;
        private readonly ILogger<PhotosController> _logger;

        public PhotosController(IRoomListCommandService commandService, ILogger<PhotosController> logger)
        {
            _commandService = commandService;
            _logger = logger;
        }

        /// <summary>
        /// Deletes a specific photo by its ID.
        /// </summary>
        /// <param name="photoId">The ID of the photo to delete.</param>
        /// <returns>NoContent if successful, NotFound if the photo does not exist, or an error response.</returns>
        [HttpDelete("{photoId}")]
        // TODO: Add authorization policy to ensure only the room owner or an admin can delete photos.
        // [Authorize(Policy = "CanDeletePhotoPolicy")] 
        public async Task<IActionResult> DeletePhoto(int photoId)
        {
            try
            {
                var success = await _commandService.DeleteRoomPhotoAsync(photoId);

                if (!success)
                {
                    return NotFound(new { message = "Photo not found." });
                }

                return NoContent(); // HTTP 204: Success, no content to return.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting photo with ID: {PhotoId}", photoId);
                return StatusCode(500, "An internal server error occurred.");
            }
        }
    }
}
