using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;
using RentalManagementPlatformWebAPI.Models;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace RentalManagementPlatformWebAPI.Services
{
    /// <summary>
    /// 房源寫入服務 - 負責處理房源資料的建立、更新、刪除等操作。
    /// 這個服務現在扮演著「生產者(Producer)」的角色，當資料變動時，它會發布訊息到 Redis Stream，
    /// 而不是直接去更新 Meilisearch 索引，以達到非同步處理和服務解耦的目的。
    /// </summary>
    public class RoomListCommandService : IRoomListCommandService
    {
        // 定義 Redis Stream 的名稱，方便統一管理。
        private const string RoomUpdatesStream = "stream:room-updates";
        private readonly IRoomListWriteRepository _writeRepository;
        private readonly RentalManagementPlatformSqlContext _context;
        // Redis 資料庫的連線實例，用於發布訊息。
        private readonly IDatabase _redisDatabase;

        public RoomListCommandService(IRoomListWriteRepository writeRepository, RentalManagementPlatformSqlContext context, IConnectionMultiplexer redis)
        {
            _writeRepository = writeRepository;
            _context = context;
            // 從 IConnectionMultiplexer 取得 Redis 資料庫的存取物件。
            _redisDatabase = redis.GetDatabase();
        }

        public async Task<RoomList> CreateRoomAsync(CreateRoomRequestDto dto)
        {
            var roomList = new RoomList
            {
                Title = dto.Title,
                Description = dto.Description,
                MaxGuests = dto.MaxGuests,
                PricePerNight = dto.PricePerNight,
                Status = "Available",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _writeRepository.AddAsync(roomList);
            await _writeRepository.SaveChangesAsync();

            // 將房源變動的訊息發布到 Redis Stream，讓背景服務去處理索引更新。
            // 這樣做可以讓 API 請求立即返回，提高回應速度。
            var roomEvent = new RoomEventDto
            {
                RoomId = roomList.RoomId,
                EventType = RoomEventType.Created,
                OccurredAt = DateTime.UtcNow,
                // TODO: 實際應用中可替換為當前操作者的 User ID
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

            // 同樣地，在更新後也發布訊息到 Redis Stream。
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

                // 刪除(軟刪除)操作也需要通知搜尋引擎更新索引。
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

        /// <summary>
        /// 新增一筆房源照片，並自動校正排序號碼(SortOrder)，確保其連續且不重複。
        /// </summary>
        /// <param name="roomPhoto">包含新照片資訊的實體物件</param>
        public async Task AddRoomPhotoAsync(RoomPhoto roomPhoto)
        {
            if (!roomPhoto.RoomId.HasValue)
            {
                throw new ArgumentException("RoomId is required when adding a room photo.", nameof(roomPhoto));
            }

            var roomId = roomPhoto.RoomId.Value;

            // 使用資料庫交易來確保整個「新增+重新排序」過程的原子性，要麼全部成功，要麼全部失敗回滾。
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 步驟 1: 先將新圖片的資料存入資料庫，這樣它才會被納入接下來的排序計算中。
                _context.RoomPhotos.Add(roomPhoto);
                await _context.SaveChangesAsync();

                // 步驟 2: 取得這個房源的「所有」圖片，並根據前端期望的順序(SortOrder)進行初步排序。
                //         使用 ThenBy(p => p.PhotoId) 是為了在 SortOrder 重複時，有一個穩定的次要排序依據。
                var photos = await _context.RoomPhotos
                    .Where(p => p.RoomId == roomId)
                    .OrderBy(p => p.SortOrder.GetValueOrDefault()) 
                    .ThenBy(p => p.PhotoId)   
                    .ToListAsync();

                // 步驟 3: 遍歷整個列表，從 0 開始重新賦予 SortOrder，確保順序是連續且唯一的。
                for (int i = 0; i < photos.Count; i++)
                {
                    photos[i].SortOrder = i;
                }

                // 步驟 4: 將所有排序校正後的變動一次性儲存到資料庫。
                await _context.SaveChangesAsync();

                // 步驟 5: 提交交易，確認所有操作成功。
                await transaction.CommitAsync();

                // 步驟 6: 照片新增成功後，發布一個事件通知 Meilisearch 更新該房源的索引，
                //        特別是考慮到封面圖片可能已經變更，需要反映在搜尋結果中。
                var roomEvent = new RoomEventDto
                {
                    RoomId = roomId,
                    EventType = RoomEventType.PhotoAdded,
                    OccurredAt = DateTime.UtcNow,
                    TriggeredBy = "System" 
                };
                await _redisDatabase.StreamAddAsync(RoomUpdatesStream, "data", JsonSerializer.Serialize(roomEvent));
            }
            catch (Exception)
            {
                // 如果中間發生任何錯誤，則回滾交易，取消所有變動，保護資料庫的一致性。
                await transaction.RollbackAsync();
                throw; // 將例外往上拋，讓上層知道操作失敗。
            }
        }
    }
}
