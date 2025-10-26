using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization; // Assuming authorization will be handled

namespace RentalManagementPlatformWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize] // Apply authorization if needed
    public class RoomsController : ControllerBase
    {
        private readonly IRoomListQueryService _queryService;
        private readonly IRoomListCommandService _commandService;
        private readonly IMinioService _minioService; // Assuming IMinioService will be in API's Services.Interfaces

        public RoomsController(IRoomListQueryService queryService, IRoomListCommandService commandService, IMinioService minioService)
        {
            _queryService = queryService;
            _commandService = commandService;
            _minioService = minioService;
        }

        // GET: api/Rooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomSummaryResponseDto>>> GetRoomSummaries()
        {
            var roomSummaries = await _queryService.GetRoomSummariesAsync();
            return Ok(roomSummaries);
        }

        // GET: api/Rooms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomDetailsResponseDto>> GetRoomDetails(int id)
        {
            var roomDetails = await _queryService.GetRoomDetailsAsync(id);
            if (roomDetails == null)
            {
                return NotFound();
            }
            return Ok(roomDetails);
        }

        // POST: api/Rooms
        [HttpPost]
        public async Task<ActionResult<RoomDetailsResponseDto>> CreateRoom([FromForm] CreateRoomRequestDto dto) // Use FromForm for file uploads
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var room = await _commandService.CreateRoomAsync(dto);
            // Assuming CreateRoomAsync returns the created RoomList entity,
            // you might want to fetch the full details or return a simplified DTO
            var createdRoomDetails = await _queryService.GetRoomDetailsAsync(room.RoomId);
            return CreatedAtAction(nameof(GetRoomDetails), new { id = room.RoomId }, createdRoomDetails);
        }

        // PUT: api/Rooms/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromForm] UpdateRoomRequestDto dto) // Use FromForm for file uploads
        {
            if (id != dto.RoomId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if room exists before attempting to update
            if (!await _queryService.RoomListExistsAsync(id))
            {
                return NotFound();
            }

            await _commandService.UpdateRoomAsync(id, dto);
            return NoContent(); // 204 No Content
        }

        // DELETE: api/Rooms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            if (!await _queryService.RoomListExistsAsync(id))
            {
                return NotFound();
            }

            await _commandService.DeleteRoomAsync(id);
            return NoContent(); // 204 No Content
        }

        // POST: api/Rooms/{id}/upload-image
        [HttpPost("{id}/upload-image")]
        public async Task<IActionResult> UploadImage(int id, IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("Please select a file to upload.");
            }

            // Check if room exists
            if (!await _queryService.RoomListExistsAsync(id))
            {
                return NotFound();
            }

            try
            {
                // Assuming _minioService.UploadFileAsync handles the upload and returns a URL or object key
                // You might need to adapt this based on the actual IMinioService implementation
                await _minioService.UploadFileAsync(imageFile.OpenReadStream(), imageFile.FileName);

                // Optionally, update the room's image URL in the database via command service
                // await _commandService.UpdateRoomImageUrlAsync(id, imageUrl);

                return Ok(new { Message = "Image uploaded successfully!" });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error uploading image: {ex.Message}");
            }
        }
    }
}