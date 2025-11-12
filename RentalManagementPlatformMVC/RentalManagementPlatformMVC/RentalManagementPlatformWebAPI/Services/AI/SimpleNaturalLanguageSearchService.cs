using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services.AI.Interfaces;

namespace RentalManagementPlatformWebAPI.Services.AI
{
    public class SimpleNaturalLanguageSearchService : INaturalLanguageSearchService
    {
        private readonly MeilisearchService _meilisearchService;
        
        // 台灣主要地標經緯度資料庫
        private readonly Dictionary<string, (double lat, double lng)> _landmarks = new()
        {
            { "101", (25.0330, 121.5654) },
            { "台北101", (25.0330, 121.5654) },
            { "台北車站", (25.0478, 121.5176) },
            { "西門町", (25.0422, 121.5078) },
            { "信義區", (25.0305, 121.5671) },
            { "大安區", (25.0337, 121.5439) },
            { "中山區", (25.0622, 121.5377) },
            { "中正區", (25.0325, 121.5208) },
            { "松山區", (25.0595, 121.5766) },
            { "板橋", (25.0144, 121.4625) },
            { "新莊", (25.0376, 121.4521) },
            { "三重", (25.0654, 121.4938) },
            { "士林", (25.0927, 121.5251) },
            { "北投", (25.1306, 121.5014) },
            { "內湖", (25.0809, 121.5789) },
            { "南港", (25.0539, 121.6082) },
            { "文山", (24.9924, 121.5562) }
        };

        public SimpleNaturalLanguageSearchService(MeilisearchService meilisearchService)
        {
            _meilisearchService = meilisearchService;
        }

        public async Task<SearchQueryParameters> ParseNaturalLanguageQueryAsync(string naturalLanguageQuery)
        {
            if (string.IsNullOrWhiteSpace(naturalLanguageQuery))
            {
                return new SearchQueryParameters();
            }

            var query = naturalLanguageQuery.ToLower().Trim();
            var parameters = new SearchQueryParameters();

            // 1. 解析距離資訊 (幾公里、公里、km)
            var distanceMatch = Regex.Match(query, @"(\d+)\s*(?:公里|km|k)");
            if (distanceMatch.Success)
            {
                parameters.RadiusKm = double.Parse(distanceMatch.Groups[1].Value);
            }

            // 2. 解析價格範圍 ($1000-2000、1000元到2000元、一千到兩千)
            var priceMatch = Regex.Match(query, @"\$(\d+)[-~](\d+)");
            if (priceMatch.Success)
            {
                parameters.PriceRange = new PriceRange
                {
                    Min = decimal.Parse(priceMatch.Groups[1].Value),
                    Max = decimal.Parse(priceMatch.Groups[2].Value)
                };
            }

            // 3. 解析人數 (2人、兩人、雙人)
            var guestMatch = Regex.Match(query, @"(\d+)人|雙人|單人");
            if (guestMatch.Success)
            {
                if (guestMatch.Value.Contains("雙人"))
                    parameters.MinGuests = 2;
                else if (guestMatch.Value.Contains("單人"))
                    parameters.MinGuests = 1;
                else
                    parameters.MinGuests = int.Parse(guestMatch.Groups[1].Value);
            }

            // 4. 解析地標和位置
            var locationInfo = ExtractLocationInfo(query);
            if (locationInfo.HasValue)
            {
                parameters.Latitude = locationInfo.Value.lat;
                parameters.Longitude = locationInfo.Value.lng;
            }

            // 5. 解析城市或區域
            var cityMatch = Regex.Match(query, @"(台北|新北|桃園|台中|台南|高雄|基隆|新竹|苗栗|彰化|南投|雲林|嘉義|屏東|宜蘭|花蓮|台東|澎湖|金門|馬祖)");
            if (cityMatch.Success)
            {
                parameters.City = cityMatch.Value;
            }

            // 6. 解析行政區
            var districtKeywords = new[] { "區", "鄉", "鎮", "市" };
            foreach (var keyword in districtKeywords)
            {
                var districtMatch = Regex.Match(query, $@"(\w+{keyword})");
                if (districtMatch.Success && !districtMatch.Value.Contains("台北"))
                {
                    parameters.District = districtMatch.Value;
                    break;
                }
            }

            // 7. 解析狀態 (活躍的、可預訂的、有效的)
            if (query.Contains("活躍") || query.Contains("可預訂") || query.Contains("有效"))
            {
                parameters.Status = "Active";
            }

            // 8. 提取剩餘的關鍵字作為搜尋字串
            var keywordsToRemove = new[] { "幫我", "幫忙", "查詢", "搜尋", "找", "的", "房源", "房間", "住宿", "附近", "周圍", "距離", "在", "到" };
            var cleanedQuery = query;
            foreach (var keyword in keywordsToRemove)
            {
                cleanedQuery = cleanedQuery.Replace(keyword, " ");
            }
            
            // 移除已解析的數字和單位
            if (distanceMatch.Success)
                cleanedQuery = cleanedQuery.Replace(distanceMatch.Value, " ");
            if (priceMatch.Success)
                cleanedQuery = cleanedQuery.Replace(priceMatch.Value, " ");
            if (guestMatch.Success)
                cleanedQuery = cleanedQuery.Replace(guestMatch.Value, " ");

            cleanedQuery = Regex.Replace(cleanedQuery, @"\s+", " ").Trim();
            
            if (!string.IsNullOrWhiteSpace(cleanedQuery) && cleanedQuery.Length > 1)
            {
                parameters.Query = cleanedQuery;
            }

            // 設定預設值
            if (!parameters.RadiusKm.HasValue && parameters.Latitude.HasValue)
            {
                parameters.RadiusKm = 5; // 預設5公里
            }

            return parameters;
        }

        public async Task<IEnumerable<RoomListSearchDto>> ProcessNaturalLanguageSearchAsync(string naturalLanguageQuery)
        {
            var parameters = await ParseNaturalLanguageQueryAsync(naturalLanguageQuery);
            
            // 如果有經緯度，使用附近搜尋
            if (parameters.Latitude.HasValue && parameters.Longitude.HasValue)
            {
                var results = await _meilisearchService.SearchNearbyAsync(
                    parameters.Query ?? string.Empty,
                    parameters.Latitude.Value,
                    parameters.Longitude.Value,
                    parameters.RadiusKm ?? 5,
                    parameters.Status,
                    parameters.SortByDistance
                );

                // 過濾結果
                return FilterResults(results, parameters);
            }
            else
            {
                // 使用一般搜尋
                var results = await _meilisearchService.SearchAsync(
                    parameters.Query ?? string.Empty,
                    parameters.Status
                );

                // 過濾結果
                return FilterResults(results, parameters);
            }
        }

        private (double lat, double lng)? ExtractLocationInfo(string query)
        {
            // 檢查是否包含地標
            foreach (var landmark in _landmarks)
            {
                if (query.Contains(landmark.Key.ToLower()))
                {
                    return landmark.Value;
                }
            }

            // 檢查是否包含經緯度座標
            var coordMatch = Regex.Match(query, @"(\d+\.?\d*)[,\s]\s*(\d+\.?\d*)");
            if (coordMatch.Success)
            {
                var lat = double.Parse(coordMatch.Groups[1].Value);
                var lng = double.Parse(coordMatch.Groups[2].Value);
                
                // 簡單的台灣座標驗證
                if (lat >= 21.8 && lat <= 25.3 && lng >= 119.9 && lng <= 122.0)
                {
                    return (lat, lng);
                }
            }

            return null;
        }

        private IEnumerable<RoomListSearchDto> FilterResults(IEnumerable<RoomListSearchDto> results, SearchQueryParameters parameters)
        {
            var filtered = results.AsEnumerable();

            // 過濾價格範圍
            if (parameters.PriceRange != null)
            {
                if (parameters.PriceRange.Min.HasValue)
                {
                    filtered = filtered.Where(r => r.PricePerNight >= parameters.PriceRange.Min.Value);
                }
                if (parameters.PriceRange.Max.HasValue)
                {
                    filtered = filtered.Where(r => r.PricePerNight <= parameters.PriceRange.Max.Value);
                }
            }

            // 過濾人數
            if (parameters.MinGuests.HasValue)
            {
                filtered = filtered.Where(r => r.MaxGuests >= parameters.MinGuests.Value);
            }

            // 過濾城市
            if (!string.IsNullOrWhiteSpace(parameters.City))
            {
                filtered = filtered.Where(r => 
                    r.CityName != null && 
                    r.CityName.Contains(parameters.City, StringComparison.OrdinalIgnoreCase));
            }

            // 過濾行政區
            if (!string.IsNullOrWhiteSpace(parameters.District))
            {
                filtered = filtered.Where(r => 
                    r.DistrictName != null && 
                    r.DistrictName.Contains(parameters.District, StringComparison.OrdinalIgnoreCase));
            }

            return filtered;
        }
    }
}