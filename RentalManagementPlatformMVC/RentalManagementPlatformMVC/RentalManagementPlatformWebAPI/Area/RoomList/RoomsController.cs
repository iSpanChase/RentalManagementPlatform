using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RentalManagementPlatformWebAPI;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services.Interfaces;

namespace RentalManagementPlatformWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomsController : ApiControllerBase
    {
        private readonly IRoomListQueryService _queryService;
        private readonly IRoomListCommandService _commandService;
        private readonly ILogger<RoomsController> _logger;

        public RoomsController(
            IRoomListQueryService queryService,
            IRoomListCommandService commandService,
            ILogger<RoomsController> logger)
        {
            _queryService = queryService;
            _commandService = commandService;
            _logger = logger;
        }

        [HttpPost("{id}/upload-image")]
        [Authorize]
        public async Task<IActionResult> UploadImage(int id, [FromForm] UploadImageDto uploadDto)
        {
            if (uploadDto.ImageFile == null || uploadDto.ImageFile.Length == 0)
            {
                return BadRequest("Please select a file to upload.");
            }

            if (!await _queryService.RoomListExistsAsync(id))
            {
                return NotFound("The specified room does not exist.");
            }

            try
            {
                var normalizedType = string.IsNullOrWhiteSpace(uploadDto.PhotoType) ? "General" : uploadDto.PhotoType;
                var createdPhoto = await _commandService.UploadAndAddPhotoAsync(id, uploadDto.ImageFile, normalizedType);
                return Ok(new { message = "Image uploaded successfully!", objectKey = createdPhoto.ObjectKey });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while uploading the image for room {RoomId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred during the image upload process.");
            }
        }

        // GET: api/Rooms
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RoomSummaryResponseDto>>> GetRoomSummaries()
        {
            var roomSummaries = await _queryService.GetRoomSummariesAsync();
            return Ok(roomSummaries);
        }

        // GET: api/Rooms/hot
        [HttpGet("hot")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RoomSummaryResponseDto>>> GetHotRooms()
        {
            var hotRooms = await _queryService.GetHotRoomsAsync();
            return Ok(hotRooms);
        }

        // GET: api/Rooms/host/5
        [HttpGet("host/{hostId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RoomSummaryResponseDto>>> GetRoomsByHostId(int hostId)
        {
            var rooms = await _queryService.GetRoomsByHostIdAsync(hostId);
            return Ok(rooms);
        }

        // GET: api/Rooms/host/me
        [HttpGet("host/me")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RoomSummaryResponseDto>>> GetMyRooms()
        {
            var rooms = await _queryService.GetRoomsByHostIdAsync(CurrentUserId);
            return Ok(rooms);
        }

        // GET: api/Rooms/5
        [HttpGet("{id}")]
        [AllowAnonymous]
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
        [Authorize]
        public async Task<ActionResult<RoomDetailsResponseDto>> CreateRoom([FromForm] CreateRoomRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 強制以 JWT 內的使用者 ID 作為 HostId
            dto.HostId = CurrentUserId;

            var room = await _commandService.CreateRoomAsync(dto);
            var createdRoomDetails = await _queryService.GetRoomDetailsAsync(room.RoomId);
            return CreatedAtAction(nameof(GetRoomDetails), new { id = room.RoomId }, createdRoomDetails);
        }

        // PUT: api/Rooms/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateRoom(int id, [FromForm] UpdateRoomRequestDto dto)
        {
            if (id != dto.RoomId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await _queryService.RoomListExistsAsync(id))
            {
                return NotFound();
            }

            await _commandService.UpdateRoomAsync(id, dto);
            return NoContent();
        }

        // DELETE: api/Rooms/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            if (!await _queryService.RoomListExistsAsync(id))
            {
                return NotFound();
            }

            await _commandService.DeleteRoomAsync(id);
            return NoContent();
        }
    }
}
