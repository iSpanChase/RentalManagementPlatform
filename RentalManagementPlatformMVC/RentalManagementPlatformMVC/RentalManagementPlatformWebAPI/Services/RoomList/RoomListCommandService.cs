using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using StackExchange.Redis;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Services
{
    public class RoomListCommandService : IRoomListCommandService
    {
        private const string RoomUpdatesStream = "stream:room-updates";
        private readonly IRoomListWriteRepository _writeRepository;
        private readonly RentalManagementPlatformSqlContext _context;
        private readonly IDatabase _redisDatabase;
        private readonly IMinioService _minioService;
        private readonly MinioSettings _minioSettings;
        private readonly ILogger<RoomListCommandService> _logger;

        public RoomListCommandService(
            IRoomListWriteRepository writeRepository, 
            RentalManagementPlatformSqlContext context, 
            IConnectionMultiplexer redis,
            IMinioService minioService,
            IOptions<MinioSettings> minioOptions,
            ILogger<RoomListCommandService> logger)
        {
            _writeRepository = writeRepository;
            _context = context;
            _redisDatabase = redis.GetDatabase();
            _minioService = minioService;
            _minioSettings = minioOptions.Value;
            _logger = logger;
        }

        public async Task<RoomPhoto> UploadAndAddPhotoAsync(int roomId, IFormFile file, string? photoType)
        {
            try
            {
                // Get the current photo count for the room to determine the next photo number.
                var photoCount = await _context.RoomPhotos.CountAsync(p => p.RoomId == roomId);
                var newPhotoNumber = photoCount + 1;

                // Generate a short random code to prevent potential name collisions.
                var randomCode = Guid.NewGuid().ToString().Substring(0, 6);

                // Construct the new object key according to the desired format, without the folder structure.
                var objectKey = $"{roomId}_photo_{newPhotoNumber}_{randomCode}{Path.GetExtension(file.FileName)}";

                await _minioService.UploadFileAsync(file.OpenReadStream(), objectKey);

                var roomPhoto = new RoomPhoto
                {
                    RoomId = roomId,
                    Bucket = _minioSettings.BucketName,
                    ObjectKey = objectKey,
                    ContentType = file.ContentType,
                    // SortOrder is intentionally left null. 
                    // The AddRoomPhotoAsync method will automatically calculate and set the correct order.
                    PhotoType = photoType ?? "General"
                };

                await AddRoomPhotoAsync(roomPhoto);

                return roomPhoto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading photo for room {RoomId}.", roomId);
                throw; // Re-throw the exception to be handled by the controller
            }
        }

        public async Task<RoomList> CreateRoomAsync(CreateRoomRequestDto dto)
        {
            // Step 1: Create and save the new Address entity first to get its ID
            var newAddress = new Address
            {
                DistrictId = dto.DistrictId,
                Street = dto.Street,
                Latitude = 25.0m, // Hardcoded as requested
                Longitude = 125.0m, // Hardcoded as requested
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Addresses.Add(newAddress);
            await _context.SaveChangesAsync();

            // Step 2: Create the RoomList entity with the new AddressId
            var roomList = new RoomList
            {
                Title = dto.Title,
                Description = dto.Description,
                MaxGuests = dto.MaxGuests,
                PricePerNight = dto.PricePerNight,
                HostId = dto.HostId,
                AddressId = newAddress.AddressId, // Use the newly created Address ID
                Status = "Available",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _writeRepository.AddAsync(roomList);
            await _writeRepository.SaveChangesAsync();

            var roomEvent = new RoomEventDto
            {
                RoomId = roomList.RoomId,
                EventType = RoomEventType.Created,
                OccurredAt = DateTime.UtcNow,
                TriggeredBy = "System" 
            };
            await _redisDatabase.StreamAddAsync(RoomUpdatesStream, "data", JsonSerializer.Serialize(roomEvent));

            return roomList;
        }

        public async Task UpdateRoomAsync(int id, UpdateRoomRequestDto dto)
        {
            var roomList = await _writeRepository.FindAsync(id);
            if (roomList == null)
            {
                return;
            }

            roomList.Title = dto.Title;
            roomList.Description = dto.Description;
            roomList.MaxGuests = dto.MaxGuests;
            roomList.PricePerNight = dto.PricePerNight;
            roomList.UpdatedAt = DateTime.UtcNow;

            _writeRepository.Update(roomList);
            await _writeRepository.SaveChangesAsync();

            var roomEvent = new RoomEventDto
            {
                RoomId = id,
                EventType = RoomEventType.Updated,
                OccurredAt = DateTime.UtcNow,
                TriggeredBy = "System"
            };
            await _redisDatabase.StreamAddAsync(RoomUpdatesStream, "data", JsonSerializer.Serialize(roomEvent));
        }

        public async Task DeleteRoomAsync(int id)
        {
            var roomList = await _writeRepository.FindAsync(id);
            if (roomList != null)
            {
                roomList.IsDeleted = true;
                roomList.Status = "已刪除";
                roomList.UpdatedAt = DateTime.UtcNow;
                _writeRepository.Update(roomList);
                await _writeRepository.SaveChangesAsync();

                var roomEvent = new RoomEventDto
                {
                    RoomId = id,
                    EventType = RoomEventType.Deleted,
                    OccurredAt = DateTime.UtcNow,
                    TriggeredBy = "System"
                };
                await _redisDatabase.StreamAddAsync(RoomUpdatesStream, "data", JsonSerializer.Serialize(roomEvent));
            }
        }

        public async Task AddRoomPhotoAsync(RoomPhoto roomPhoto)
        {
            if (!roomPhoto.RoomId.HasValue)
            {
                throw new ArgumentException("RoomId is required when adding a room photo.", nameof(roomPhoto));
            }

            // 找出目前該房源照片的最大排序號碼
            var maxSortOrder = await _context.RoomPhotos
                .Where(p => p.RoomId == roomPhoto.RoomId)
                .MaxAsync(p => (int?)p.SortOrder);

            // 將新照片的排序號碼設為最大值 + 1 (如果沒有任何照片，則從 0 開始)
            roomPhoto.SortOrder = (maxSortOrder ?? -1) + 1;

            // 新增照片紀錄並儲存
            _context.RoomPhotos.Add(roomPhoto);
            await _context.SaveChangesAsync();

            // 發布事件通知 Meilisearch 更新索引
            var roomEvent = new RoomEventDto
            {
                RoomId = roomPhoto.RoomId.Value,
                EventType = RoomEventType.PhotoAdded,
                OccurredAt = DateTime.UtcNow,
                TriggeredBy = "System"
            };
            await _redisDatabase.StreamAddAsync(RoomUpdatesStream, "data", JsonSerializer.Serialize(roomEvent));
        }
        public async Task<bool> DeleteRoomPhotoAsync(int photoId)
        {
            var photo = await _context.RoomPhotos.FindAsync(photoId);
            if (photo == null)
            {
                _logger.LogWarning("Attempted to delete a non-existent photo with ID: {PhotoId}", photoId);
                return false;
            }

            try
            {
                // Step 1: Delete the file from MinIO storage
                await _minioService.DeleteFileAsync(photo.ObjectKey);

                // Step 2: Remove the photo record from the database
                _context.RoomPhotos.Remove(photo);
                await _context.SaveChangesAsync();

                // Optional: Here you could re-sort the remaining photos if needed, 
                // but for performance, it might be better to handle gaps in the ordering on the client-side.

                _logger.LogInformation("Successfully deleted photo with ID: {PhotoId} and ObjectKey: {ObjectKey}", photoId, photo.ObjectKey);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting photo with ID: {PhotoId}", photoId);
                // We re-throw the exception so the controller can handle the HTTP response.
                // Depending on the policy, you might want to handle the case where the file is deleted from MinIO
                // but the DB deletion fails.
                throw;
            }
        }
    }
}
