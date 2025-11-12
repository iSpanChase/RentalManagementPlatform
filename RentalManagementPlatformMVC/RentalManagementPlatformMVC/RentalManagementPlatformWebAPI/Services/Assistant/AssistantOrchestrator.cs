using System.Text.Json;
using System.Text.Json.Nodes;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace RentalManagementPlatformWebAPI.Services.Assistant
{
    public class AssistantOrchestrator
    {
        private readonly IPlacesService _places;
        private readonly MeilisearchService _meili;
        private readonly IQwenClient _qwen;
        private readonly QwenOptions _qwenOptions;
        private readonly ILogger<AssistantOrchestrator> _logger;

        public AssistantOrchestrator(IPlacesService places, MeilisearchService meili, IQwenClient qwen, IOptions<QwenOptions> qwenOptions, ILogger<AssistantOrchestrator> logger)
        {
            _places = places;
            _meili = meili;
            _qwen = qwen;
            _qwenOptions = qwenOptions.Value;
            _logger = logger;
        }

        /// <summary>
        /// 從自然語言中提取搜尋關鍵字
        /// </summary>
        private string ExtractKeywords(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return string.Empty;
            
            // 移除常見的搜尋語助詞
            var noiseWords = new[] { "附近", "周邊", "旁邊", "公里", "km", "KM", 
                                   "房源", "房間", "住宿", "飯店", "酒店", 
                                   "幫我", "幫你", "請", "謝謝",
                                   "查詢", "搜尋", "尋找", "找", "看", 
                                   "的", "了", "吧", "啊", "呢", "嗎" };
            
            var cleaned = query;
            foreach (var noise in noiseWords)
            {
                cleaned = cleaned.Replace(noise, " ");
            }
            
            // 移除數字和距離相關詞
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\d+", " ");
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"[0-9]+\s*公里", " ");
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"[0-9]+\s*km", " ");
            
            // 清理多餘空格
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", " ").Trim();
            
            // 如果清理後為空，返回原始查詢的一部分（地名）
            if (string.IsNullOrWhiteSpace(cleaned))
            {
                // 嘗試提取地名 - 假設地名在句子前面部分
                var parts = query.Split(new[] { "附近", "周邊", "旁邊", "公里", "km" }, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    return parts[0].Trim();
                }
                return string.Empty;
            }
            
            return cleaned;
        }

        public async Task<(string message, object data)> ChatAsync(string text, CancellationToken ct)
        {
            _logger.LogInformation("=== AI搜尋流程開始 ===");
            _logger.LogInformation("用戶輸入: {Text}", text);
            
            var tools = BuildToolsSchema();

            if (string.IsNullOrWhiteSpace(_qwenOptions.ApiKey) || string.IsNullOrWhiteSpace(_qwenOptions.Model))
            {
                _logger.LogWarning("Qwen API未配置，使用fallback模式");
                // Fallback when Qwen not configured: minimal deterministic path
                var fallbackPlace = string.IsNullOrWhiteSpace(text) ? "台北101" : text.Trim();
                _logger.LogInformation("Fallback搜尋地點: {Place}", fallbackPlace);
                
                var suggestions = await _places.SearchBroadAsync(fallbackPlace, 5, ct);
                _logger.LogInformation("Places搜尋返回 {Count} 個結果", suggestions.Count);
                
                var place = suggestions.FirstOrDefault();
                if (place == null)
                {
                    _logger.LogError("找不到地點: {Place}", fallbackPlace);
                    return ($"找不到與『{fallbackPlace}』相符的地點，請換個關鍵詞或提供更明確的地標。", new { place = (object?)null, rooms = Array.Empty<RoomListSearchDto>(), radiusKm = 5d, sortByDistance = true });
                }

                _logger.LogInformation("找到地點: {Name}, 座標: {Lat}, {Lng}", place.Name, place.Lat, place.Lng);
                var rooms = (await _meili.SearchNearbyAsync(string.Empty, place.Lat ?? 0, place.Lng ?? 0, 5, "Active", true)).ToList();
                _logger.LogInformation("房源搜尋返回 {Count} 個結果", rooms.Count);
                
                var msg = rooms.Any()
                    ? $"在『{place.Name}』附近 5 公里內，共找到 {rooms.Count} 間房源。"
                    : $"在『{place.Name}』附近 5 公里內沒有找到房源。";
                return (msg, new { place = new { place.Name, place.Lat, place.Lng }, rooms, radiusKm = 5d, sortByDistance = true });
            }

            var messages = new List<QwenMessage>
            {
                new QwenMessage("system", "你是旅宿搜尋助手，善用可用工具完成使用者意圖。若使用者提供地名，先呼叫 places_search 取得座標，再用 search_rooms_nearby 查詢房源。所有操作必須為只讀。"),
                new QwenMessage("user", text ?? string.Empty)
            };

            IReadOnlyList<PlaceSuggestionDto>? lastPlaces = null;
            IReadOnlyList<RoomListSearchDto>? lastRooms = null;
            double lastRadius = 5;
            bool lastSort = true;
            var maxTurns = 3;

            for (var turn = 0; turn < maxTurns; turn++)
            {
                var req = new QwenChatRequest
                {
                    Model = _qwenOptions.Model!,
                    Messages = messages,
                    Tools = tools,
                    ToolChoice = "auto"
                };

                var resp = await _qwen.CreateChatCompletionAsync(req, ct);
                var msg = resp.Choices.FirstOrDefault()?.Message;
                if (msg == null) break;

                if (msg.ToolCalls != null && msg.ToolCalls.Count > 0)
                {
                    foreach (var call in msg.ToolCalls)
                    {
                        var name = call.Function?.Name;
                        var argsJson = call.Function?.Arguments ?? "{}";
                        var args = JsonNode.Parse(argsJson) as JsonObject ?? new JsonObject();

                        if (string.Equals(name, "places_search", StringComparison.OrdinalIgnoreCase))
                        {
                            var q = args["q"]?.GetValue<string>() ?? string.Empty;
                            _logger.LogInformation("=== 地標搜尋開始 ===");
                            _logger.LogInformation("Qwen AI解析出的地標查詢: {Query}", q);
                            
                            lastPlaces = await _places.SearchBroadAsync(q, 5, ct);
                            _logger.LogInformation("地標搜尋返回 {Count} 個結果", lastPlaces?.Count ?? 0);
                            
                            if (lastPlaces?.Any() == true)
                            {
                                var firstPlace = lastPlaces.First();
                                _logger.LogInformation("第一個地標結果: {Name}, 座標: {Lat}, {Lng}", firstPlace.Name, firstPlace.Lat, firstPlace.Lng);
                            }
                            else
                            {
                                _logger.LogWarning("地標搜尋沒有返回任何結果");
                            }
                            
                            var payload = JsonSerializer.SerializeToNode(lastPlaces)!.ToJsonString();
                            messages.Add(QwenMessage.Tool(call.Id!, payload));
                        }
                        else if (string.Equals(name, "search_rooms_nearby", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogInformation("=== 房源搜尋開始 ===");
                            
                            var lat = args["lat"]?.GetValue<double>() ?? 0;
                            var lng = args["lng"]?.GetValue<double>() ?? 0;
                            var radiusKm = args["radiusKm"]?.GetValue<double>() ?? 5;
                            var query = args["query"]?.GetValue<string>() ?? string.Empty;
                            // 智能清理查詢詞語 - 提取關鍵字而不是完全清空
                            if (!string.IsNullOrWhiteSpace(query))
                            {
                                var s = query.Trim();
                                _logger.LogInformation("原始查詢詞語: {OriginalQuery}", s);
                                
                                // 提取可能的關鍵字（地名、設施等）
                                var keywords = ExtractKeywords(s);
                                _logger.LogInformation("提取關鍵字: {Keywords} (原始: {Original})", keywords, s);
                                query = keywords;
                            }
                            // 放寬狀態過濾，避免與索引內中文狀態（如「已通過」）不一致
                            var status = args["status"]?.GetValue<string>() ?? "";
                            if (string.IsNullOrWhiteSpace(status)) status = ""; // 使用空字符串表示不過濾狀態
                            var sortByDistance = args["sortByDistance"]?.GetValue<bool?>() ?? true;

                            _logger.LogInformation("AI解析參數 - 座標: ({Lat}, {Lng}), 半徑: {RadiusKm}km, 查詢: '{Query}', 狀態: '{Status}', 距離排序: {SortByDistance}", 
                                lat, lng, radiusKm, query, status, sortByDistance);
                            
                            // 重要：記錄狀態過濾變更
                            if (string.IsNullOrWhiteSpace(status))
                            {
                                _logger.LogWarning("【重要更新】AI搜尋已移除狀態過濾，將顯示所有未刪除的房源 (is_deleted = false)");
                            }
                            
                            // 特別記錄狀態過濾相關資訊
                            if (string.IsNullOrWhiteSpace(status))
                            {
                                _logger.LogInformation("狀態過濾: 不過濾狀態 (已移除狀態過濾，只保留 is_deleted = false 的基本過濾)");
                            }
                            else
                            {
                                _logger.LogInformation("狀態過濾: 只顯示狀態為 '{Status}' 的房源 (注意：AI搜尋建議使用空字符串來顯示所有未刪除房源)", status);
                            }

                            lastRadius = radiusKm; lastSort = sortByDistance;
                            lastRooms = (await _meili.SearchNearbyAsync(query, lat, lng, radiusKm, status, sortByDistance)).ToList();
                            _logger.LogInformation("初次房源搜尋返回 {Count} 個結果", lastRooms?.Count ?? 0);

                            // Auto-widen radius and relax status when no results
                            if (lastRooms.Count == 0)
                            {
                                _logger.LogInformation("初次搜尋無結果，開始自動擴大範圍 (當前半徑: {CurrentRadius}km)", radiusKm);
                                
                                // 從較小的半徑開始逐步擴大，避免一開始就用50km
                                var baseRadius = Math.Min(radiusKm, 5); // 從5km或更小開始
                                var trials = new double[] { baseRadius, 10, 15, 25, 50 };
                                
                                foreach (var r in trials)
                                {
                                    if (r <= radiusKm) continue; // 跳過已經試過的半徑
                                    
                                    _logger.LogInformation("嘗試擴大範圍至 {Radius}km", r);
                                    var retry = (await _meili.SearchNearbyAsync(query, lat, lng, r, string.Empty, sortByDistance)).ToList();
                                    _logger.LogInformation("擴大至 {Radius}km 後返回 {Count} 個結果", r, retry.Count);
                                    if (retry.Count > 0)
                                    {
                                        lastRadius = r;
                                        lastRooms = retry;
                                        _logger.LogInformation("找到結果，使用擴大後的範圍: {Radius}km", r);
                                        break;
                                    }
                                }
                            }
                            var payload = JsonSerializer.SerializeToNode(lastRooms)!.ToJsonString();
                            messages.Add(QwenMessage.Tool(call.Id!, payload));
                        }
                    }

                    // Continue loop to let model summarize
                    continue;
                }

                // No tool calls; return final message
                var finalText = msg.Content ?? "";
                _logger.LogInformation("=== 最終結果準備返回 ===");
                _logger.LogInformation("最終訊息: {FinalText}", finalText);
                _logger.LogInformation("最終地點: {Place}", lastPlaces?.FirstOrDefault()?.Name ?? "無");
                _logger.LogInformation("最終房源數量: {RoomCount}", lastRooms?.Count ?? 0);
                _logger.LogInformation("最終半徑: {Radius}km", lastRadius);
                
                return (finalText, new
                {
                    place = lastPlaces?.FirstOrDefault() is { } p ? new { p.Name, p.Lat, p.Lng } : null,
                    rooms = lastRooms ?? Array.Empty<RoomListSearchDto>(),
                    radiusKm = lastRadius,
                    sortByDistance = lastSort
                });
            }

            // Safety fallback
            return ("目前無法完成查詢，請稍後再試。", new { place = (object?)null, rooms = Array.Empty<RoomListSearchDto>(), radiusKm = 5d, sortByDistance = true });
        }

        private static List<object> BuildToolsSchema()
        {
            return new List<object>
            {
                new {
                    type = "function",
                    function = new {
                        name = "places_search",
                        description = "Read-only. 將地名/地標（如台北101）轉成座標。當使用者提供地名而非經緯度時先呼叫此函數，最多回傳5筆。",
                        parameters = new {
                            type = "object",
                            properties = new {
                                q = new { type = "string", description = "地名或地標（任意語言）。" }
                            },
                            required = new [] { "q" }
                        }
                    }
                },
                new {
                    type = "function",
                    function = new {
                        name = "search_rooms_nearby",
                        description = "Read-only. 以經緯度與半徑（公里）搜尋附近房源。若只提供地名請先呼叫 places_search。sortByDistance=true 會依距離排序。",
                        parameters = new {
                            type = "object",
                            properties = new {
                                lat = new { type = "number", description = "緯度（-90..90）。" },
                                lng = new { type = "number", description = "經度（-180..180）。" },
                                radiusKm = new { type = "number", description = "半徑（公里，0.5–50，預設5）。" },
                                query = new { type = "string", description = "可選關鍵字。" },
                                status = new { type = "string", description = "狀態過濾，空值表示不過濾狀態。" },
                                sortByDistance = new { type = "boolean", description = "是否依距離排序，預設 true。" }
                            },
                            required = new [] { "lat", "lng", "radiusKm" }
                        }
                    }
                }
            };
        }
    }
}
