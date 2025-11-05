using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Area.ReportForm.DTO.Recommendation;
using RentalManagementPlatformWebAPI.Models;
using StackExchange.Redis;
using System;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using System.Text.Json;

namespace RentalManagementPlatformWebAPI.Area.ReportForm.Services
{
    public class RecommendationService
    {
        private readonly RentalManagementPlatformSqlContext _context;
        private readonly IDatabase _redisDb;
        private readonly IFileUrlResolver _urlResolver; // Assuming IFileUrlResolver will be in API's Services.Interfaces
        private const string RecommendationCachePrefix = "recommendation:user:";
        private const string RecommendationPopularCachePrefix = "recommendation:popular:";

        public RecommendationService(RentalManagementPlatformSqlContext context, IConnectionMultiplexer redis, IFileUrlResolver urlResolver)
        {
            _context = context;
            _redisDb = redis.GetDatabase();
            _urlResolver = urlResolver;
        }

        public async Task<HashSet<int>> GetPopularRoomId(int topN, string? gender = null)
        {
            // 1. 決定在 Redis 中儲存的 Key
            var cacheKey = $"{RecommendationPopularCachePrefix}{gender ?? ""}";

            // 2. 嘗試從 Redis 獲取快取資料
            var cachedRecommendations = await _redisDb.StringGetAsync(cacheKey);

            // 如果快取命中，反序列化 JSON 並直接回傳
            if (cachedRecommendations.HasValue)
            {
                var cachedRecommendationsObject
                    = JsonSerializer.Deserialize<List<int>>(cachedRecommendations!)
                    ?? new List<int>();
                if (cachedRecommendationsObject.Count > 0)
                    return cachedRecommendationsObject.ToHashSet();
            }

            // --- 如果快取中沒有，則執行以下計算 ---

            // 3. 執行推薦演算法
            var calculatePopularRooms = await CalculatePopularRooms(topN, gender);

            // 4. 將計算結果序列化成 JSON 並存入 Redis
            var expiry = TimeSpan.FromHours(24);//24hr後逾期
            var serializedResult = JsonSerializer.Serialize(calculatePopularRooms);
            await _redisDb.StringSetAsync(cacheKey, serializedResult, expiry);

            // 5. 回傳計算結果
            return calculatePopularRooms.ToHashSet();
        }

        private async Task<List<int>> CalculatePopularRooms(int topN, string? gender)
        {
            return await _context.Bookings
                .Where(b => gender == null || b.Guest.Gender == gender)
                .GroupBy(b => b.RoomId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Join(_context.RoomLists,
                        roomId => roomId,
                        room => room.RoomId,
                        (roomId, room) => room)
                .Where(r => !r.IsDeleted && r.Status == "上架中")
                .Select(r => r.RoomId)
                .OfType<int>()
                .Take(topN)
                .ToListAsync();
        }

        public async Task<List<RecommendedRoomDto>> GetRecommendationsForGuest(int? guestId =null, int topN = 20)
        {
            // 1. 決定在 Redis 中儲存的 Key
            var cacheKey = guestId.HasValue
                ? $"{RecommendationCachePrefix}{guestId.Value}"
                : $"{RecommendationCachePrefix}anonymous";

            // 2. 嘗試從 Redis 獲取快取資料
            var cachedRecommendations = await _redisDb.StringGetAsync(cacheKey);
            
            // 如果快取命中，反序列化 JSON 並直接回傳
            if (cachedRecommendations.HasValue)
            {
                var cachedRecommendationsObject
                    = JsonSerializer.Deserialize<List<RecommendedRoomDto>>(cachedRecommendations!)
                    ?? new List<RecommendedRoomDto>();
                if(cachedRecommendationsObject.Count > 0)
                    return cachedRecommendationsObject;
            }

            List<RecommendedRoomDto> recommendations;

            if (guestId.HasValue)
            {
                // 已登入用戶：執行個人化推薦演算法
                recommendations = await CalculateRecommendations(guestId.Value, topN);
            }
            else
            {
                // 未登入用戶：回傳全站熱門房源
                var popularRoomIds = await GetPopularRoomId(topN);

                recommendations = (
                    await _context.RoomLists
                     .Where(r => popularRoomIds.Contains(r.RoomId) && r.Address != null)
                     .Include(r => r.Address).ThenInclude(a => a.District).ThenInclude(d => d.City)
                      .ToListAsync()
                      )
                      .Select(
                     r => new RecommendedRoomDto
                     {
                         RoomId = r.RoomId,
                         Title = r.Title ?? "N/A",
                         PricePerNight = r.PricePerNight ?? 0,
                         AddressLine = $"{r.Address?.District?.City?.CityName}{r.Address?.District?.DistrictName}{r.Address?.Street}",
                         CityName = r.Address?.District?.City?.CityName,
                         DistrictName = r.Address?.District?.DistrictName,
                         Street = r.Address?.Street
                     }
                    ).ToList();
                foreach ( var r in recommendations)
                {
                    var photoUrls = await _urlResolver.GetRoomPhotoUrlsAsync(r.RoomId);
                    r.mainImageUrl = photoUrls.ToList().FirstOrDefault();
                }
            }

            // 4. 計算過期時間並存入 Redis 
            var expiry = TimeSpan.FromHours(1);
            var serializedResult = JsonSerializer.Serialize(recommendations);
            await _redisDb.StringSetAsync(cacheKey, serializedResult, expiry);

            // 5. 回傳計算結果
            return recommendations;
        }

        private async Task<List<RecommendedRoomDto>> CalculateRecommendations(int guestId, int topN)
        {
            Console.WriteLine($"Calculating recommendations for guest {guestId}...");

            // 1a. 獲取使用者基本資料 (性別)
            var guest = await _context.Users.FindAsync(guestId);
            //判斷使用者是否存在，若無，回傳空結果
            bool isGuest = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == guestId);
            if (!isGuest ) return new List<RecommendedRoomDto>();

            // 1b. 獲取使用者的歷史訂單，並從中分析出偏好
            var pastBookings = await _context.Bookings
                .Where(b => b.GuestId == guestId && (b.Status == "Completed" || b.Status == "Confirmed"))
                .Include(b => b.Room)
                .ThenInclude(r => r.Address)
                .ThenInclude(a => a.District)
                .ToListAsync();

            var pastRoomIds = pastBookings.Select(b => b.RoomId).OfType<int>().ToHashSet();
            var pastDistrictIds = pastBookings.Select(b => b.Room?.Address?.DistrictId).OfType<int>().ToHashSet();
            var pastCityIds = pastBookings.Select(b => b.Room?.Address?.District?.CityId).OfType<int>().ToHashSet();

            // 計算平均房價，如果沒有歷史訂單，則給一個預設值 (例如 2500)
            decimal avgPrice = pastBookings.Any()
                ? pastBookings.Average(b => b.Room?.PricePerNight ?? 0)
                : 2500m;

            // 1c. 獲取其他使用者所喜愛的房源 (取預訂次數最多的前 10 名)
            var popularRoomIds = await GetPopularRoomId(topN);

            // 1d. 獲取與使用者同性別的其他使用者所喜愛的房源 (取預訂次數最多的前 10 名)
            var sameGenderPopularRoomIds =
                guest.Gender == null ? new HashSet<int>() : await GetPopularRoomId(topN, guest.Gender);

            // 1e. 獲取所有可被推薦的候選房源
            // 條件：未被刪除、已上架
            var candidateRooms = await _context.RoomLists
                .Where(r => !r.IsDeleted && r.Status == "上架中")
                .Include(r => r.Address)
                .ThenInclude(a => a.District)
                .ThenInclude(d => d.City)
                .ToListAsync();


            // 為每個候選房源評分
            var scoredRooms = new List<(RecommendedRoomDto room, double score)>();

            foreach (var room in candidateRooms)
            {
                double score = 0;

                // 規則 1: 地區加權 - 住過的行政區 > 城市
                if (room.Address?.DistrictId != null && pastDistrictIds.Contains(room.Address.DistrictId.Value))
                {
                    score += 30; // 相同的行政區，高分
                }
                else if (room.Address?.District?.CityId != null && pastCityIds.Contains(room.Address.District.CityId.Value))
                {
                    score += 15; // 相同的城市，次高分
                }

                // 規則 2: 熱門房源加權
                if (popularRoomIds.Contains(room.RoomId))
                {
                    score += 20;
                }

                // 規則 3: 同性別偏好加權
                if (sameGenderPopularRoomIds.Contains(room.RoomId))
                {
                    score += 10;
                }

                // 規則 4: 價格加權 - 越接近使用者平均房價，分數越高
                decimal priceDifference = Math.Abs((room.PricePerNight ?? 2500m) - avgPrice);
                // 使用價格差異百分比來做懲罰，避免價格高的房源被過度懲罰
                double pricePenalty = (double)(priceDifference / avgPrice) * 10;
                score -= pricePenalty;

                // 將計算結果加入列表
                scoredRooms.Add((new RecommendedRoomDto
                {
                    RoomId = room.RoomId,
                    Title = room.Title ?? "N/A",
                    PricePerNight = room.PricePerNight ?? 0,
                    AddressLine = $"{room.Address?.District?.City?.CityName}{room.Address?.District?.DistrictName}{room.Address?.Street}",
                    CityName = room.Address?.District?.City?.CityName,
                    DistrictName = room.Address?.District?.DistrictName,
                    Street = room.Address?.Street,
                    // 圖片 URL 最後再統一處理，避免在迴圈中查詢資料庫
                }, score));
            }

            //排序並選出前 N 名
            var topRooms = scoredRooms
                .OrderByDescending(r => r.score)
                .Take(topN)
                .Select(r => r.room)
                .ToList();

            // 為選出的 Top N 房源，補上圖片 URL
            foreach (var room in topRooms)
            {
                var photoUrls = await _urlResolver.GetRoomPhotoUrlsAsync(room.RoomId);
                room.mainImageUrl = photoUrls.FirstOrDefault();
            }

            return topRooms;
        }
    }
}