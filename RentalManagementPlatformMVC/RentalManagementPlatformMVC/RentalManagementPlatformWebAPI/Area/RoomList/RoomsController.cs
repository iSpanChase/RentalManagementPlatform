using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // Assuming authorization will be handled
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Services.Interfaces;

namespace RentalManagementPlatformWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize] // Apply authorization if needed
    public class RoomsController : ControllerBase
    {
        private readonly IRoomListQueryService _queryService;
        private readonly IRoomListCommandService _commandService;
        private readonly IMinioService _minioService;
        private readonly MinioSettings _minioSettings;
        private readonly ILogger<RoomsController> _logger;

        public RoomsController(
            IRoomListQueryService queryService,
            IRoomListCommandService commandService,
            IMinioService minioService,
            // 使用 IOptions<T> 模式來讀取 appsettings.json 中的 Minio 設定
            IOptions<MinioSettings> minioOptions,
            // 注入 ASP.NET Core 的標準日誌記錄器
            ILogger<RoomsController> logger)
        {
            _queryService = queryService;
            _commandService = commandService;
            _minioService = minioService;
            _logger = logger;
            _minioSettings = minioOptions.Value;
        }

        // GET: api/Rooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomSummaryResponseDto>>> GetRoomSummaries()
        {
            var roomSummaries = await _queryService.GetRoomSummariesAsync();
            return Ok(roomSummaries);
        }

        // GET: api/Rooms/hot
        [HttpGet("hot")]
        public async Task<ActionResult<IEnumerable<RoomSummaryResponseDto>>> GetHotRooms()
        {
            var hotRooms = await _queryService.GetHotRoomsAsync();
            return Ok(hotRooms);
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

        /// <summary>
        /// 上傳圖片到指定的房源。
        /// </summary>
        /// <param name="id">房源 ID</param>
        /// <param name="uploadDto">圖片上傳的資料傳輸物件，包含圖片檔案和其中繼資料。</param>
        /// <returns></returns>
        [HttpPost("{id}/upload-image")]
        public async Task<IActionResult> UploadImage(int id, [FromForm] UploadImageDto uploadDto)
        {
            // 透過 [FromForm] 標籤，ASP.NET Core 會自動將 multipart/form-data 請求的內容綁定到 uploadDto 物件上。
            if (uploadDto.ImageFile == null || uploadDto.ImageFile.Length == 0)
            {
                return BadRequest("請選擇要上傳的檔案。");
            }

            if (!await _queryService.RoomListExistsAsync(id))
            {
                return NotFound("指定的房源不存在。");
            }

            try
            {
                // 步驟 1: 產生一個唯一的檔案名稱(ObjectKey)，以避免檔名衝突和惡意路徑攻擊。
                var objectKey = $"{Guid.NewGuid()}{Path.GetExtension(uploadDto.ImageFile.FileName)}";

                // 步驟 2: 呼叫 Minio 服務，將檔案串流和新的 ObjectKey 傳遞過去，進行上傳。
                await _minioService.UploadFileAsync(uploadDto.ImageFile.OpenReadStream(), objectKey);

                // 步驟 3: 建立資料庫實體 RoomPhoto，將來自 DTO 和其他地方的資料打包起來。
                var roomPhoto = new RoomPhoto
                {
                    RoomId = id,
                    Bucket = _minioSettings.BucketName,
                    ObjectKey = objectKey,
                    ContentType = uploadDto.ImageFile.ContentType,
                    SortOrder = uploadDto.SortOrder,
                    // 如果前端沒有提供 PhotoType，則給定一個預設值 "General"。
                    PhotoType = uploadDto.PhotoType ?? "General"
                };

                // 步驟 4: 呼叫 Command 服務，將圖片的元資料寫入資料庫，並處理排序邏輯。
                await _commandService.AddRoomPhotoAsync(roomPhoto);

                return Ok(new { message = "圖片上傳成功！", objectKey });
            }
            catch (Exception ex)
            {
                // 記錄詳細的錯誤日誌，但只回傳一個通用的錯誤訊息給前端，避免洩漏內部實作細節。
                _logger.LogError(ex, "上傳圖片至房源 {RoomId} 時發生錯誤。", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "圖片上傳過程中發生內部錯誤。");
            }
        }
    }
}