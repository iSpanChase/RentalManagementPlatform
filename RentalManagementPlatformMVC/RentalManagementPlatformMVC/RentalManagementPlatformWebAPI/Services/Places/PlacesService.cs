using Microsoft.Extensions.Configuration;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace RentalManagementPlatformWebAPI.Services.Places
{
    public class PlacesService : IPlacesService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<PlacesService> _logger;

        public PlacesService(IConfiguration config, IHttpClientFactory httpFactory, ILogger<PlacesService> logger)
        {
            _config = config;
            _httpFactory = httpFactory;
            _logger = logger;
        }

        // UI-oriented: attractions/landmarks around Taipei bias
        public async Task<IReadOnlyList<PlaceSuggestionDto>> SearchAsync(string query, int take = 5, CancellationToken ct = default)
        {
            query = (query ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(query)) return Array.Empty<PlaceSuggestionDto>();

            var apiKey = _config["Google:ApiKey"] ?? Environment.GetEnvironmentVariable("GOOGLE_MAPS_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Google API key not configured");

            // Attractions/landmarks with Taipei bias (keep original UX behavior)
            var lat = 25.033964; // Taipei 101 approx
            var lng = 121.564468;
            var radius = 30000; // 30km
            var url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={Uri.EscapeDataString(query)}&type=tourist_attraction&language=zh-TW&location={lat},{lng}&radius={radius}&key={apiKey}";

            using var client = _httpFactory.CreateClient();
            using var resp = await client.GetAsync(url, ct);
            resp.EnsureSuccessStatusCode();
            await using var stream = await resp.Content.ReadAsStreamAsync(ct);

            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var root = doc.RootElement;
            if (root.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
            {
                var list = new List<PlaceSuggestionDto>();
                foreach (var item in results.EnumerateArray())
                {
                    var placeId = item.TryGetProperty("place_id", out var pid) ? pid.GetString() : null;
                    var name = item.TryGetProperty("name", out var nm) ? nm.GetString() : null;
                    double? plat = null, plng = null;
                    if (item.TryGetProperty("geometry", out var geo) &&
                        geo.TryGetProperty("location", out var loc))
                    {
                        if (loc.TryGetProperty("lat", out var latEl) && latEl.TryGetDouble(out var latVal)) plat = latVal;
                        if (loc.TryGetProperty("lng", out var lngEl) && lngEl.TryGetDouble(out var lngVal)) plng = lngVal;
                    }

                    if (!string.IsNullOrWhiteSpace(name) && plat.HasValue && plng.HasValue)
                    {
                        list.Add(new PlaceSuggestionDto
                        {
                            PlaceId = placeId,
                            Name = name!,
                            Lat = plat,
                            Lng = plng
                        });
                    }
                }

                return list.Take(Math.Max(1, take)).ToList();
            }
            return Array.Empty<PlaceSuggestionDto>();
        }

        // Broad: addresses/administrative areas with Geocoding fallback
        public async Task<IReadOnlyList<PlaceSuggestionDto>> SearchBroadAsync(string query, int take = 5, CancellationToken ct = default)
        {
            query = (query ?? string.Empty).Trim();
            _logger.LogInformation("=== PlacesService.SearchBroadAsync 開始 ===");
            _logger.LogInformation("搜尋查詢: {Query}, take: {Take}", query, take);
            
            if (string.IsNullOrWhiteSpace(query)) 
            {
                _logger.LogWarning("查詢為空，返回空結果");
                return Array.Empty<PlaceSuggestionDto>();
            }

            var apiKey = _config["Google:ApiKey"] ?? Environment.GetEnvironmentVariable("GOOGLE_MAPS_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("Google API key 未配置");
                throw new InvalidOperationException("Google API key not configured");
            }

            var url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={Uri.EscapeDataString(query)}&language=zh-TW&region=TW&key={apiKey}";
            _logger.LogInformation("Google Places API URL: {Url}", url.Replace(apiKey, "***"));

            using var client = _httpFactory.CreateClient();
            using var resp = await client.GetAsync(url, ct);
            _logger.LogInformation("Google Places API 返回狀態碼: {StatusCode}", resp.StatusCode);
            resp.EnsureSuccessStatusCode();
            await using var stream = await resp.Content.ReadAsStreamAsync(ct);

            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var root = doc.RootElement;
            if (root.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
            {
                _logger.LogInformation("Google Places API 返回 {Count} 個結果", results.GetArrayLength());
                var list = new List<PlaceSuggestionDto>();
                foreach (var item in results.EnumerateArray())
                {
                    var placeId = item.TryGetProperty("place_id", out var pid) ? pid.GetString() : null;
                    var name = item.TryGetProperty("name", out var nm) ? nm.GetString() : null;
                    double? plat = null, plng = null;
                    if (item.TryGetProperty("geometry", out var geo) &&
                        geo.TryGetProperty("location", out var loc))
                    {
                        if (loc.TryGetProperty("lat", out var latEl) && latEl.TryGetDouble(out var latVal)) plat = latVal;
                        if (loc.TryGetProperty("lng", out var lngEl) && lngEl.TryGetDouble(out var lngVal)) plng = lngVal;
                    }

                    if (!string.IsNullOrWhiteSpace(name) && plat.HasValue && plng.HasValue)
                    {
                        list.Add(new PlaceSuggestionDto
                        {
                            PlaceId = placeId,
                            Name = name!,
                            Lat = plat,
                            Lng = plng
                        });
                        _logger.LogInformation("找到地點: {Name}, 座標: ({Lat}, {Lng})", name, plat, plng);
                    }
                }

                if (list.Count > 0)
                {
                    _logger.LogInformation("Places搜尋成功，返回 {Count} 個結果", list.Count);
                    return list.Take(Math.Max(1, take)).ToList();
                }
            }

            _logger.LogWarning("Google Places API 無結果，嘗試 Geocoding fallback");
            // Geocoding fallback
            var geoUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(query)}&language=zh-TW&region=TW&key={apiKey}";
            _logger.LogInformation("Google Geocoding API URL: {Url}", geoUrl.Replace(apiKey, "***"));
            
            using (var geoResp = await client.GetAsync(geoUrl, ct))
            {
                _logger.LogInformation("Google Geocoding API 返回狀態碼: {StatusCode}", geoResp.StatusCode);
                geoResp.EnsureSuccessStatusCode();
                await using var geoStream = await geoResp.Content.ReadAsStreamAsync(ct);
                using var geoDoc = await JsonDocument.ParseAsync(geoStream, cancellationToken: ct);
                var geoRoot = geoDoc.RootElement;
                if (geoRoot.TryGetProperty("results", out var geoResults) && geoResults.ValueKind == JsonValueKind.Array)
                {
                    _logger.LogInformation("Google Geocoding API 返回 {Count} 個結果", geoResults.GetArrayLength());
                    var list = new List<PlaceSuggestionDto>();
                    foreach (var item in geoResults.EnumerateArray())
                    {
                        var name = item.TryGetProperty("formatted_address", out var addr) ? addr.GetString() : null;
                        double? plat = null, plng = null;
                        if (item.TryGetProperty("geometry", out var geo) &&
                            geo.TryGetProperty("location", out var loc))
                        {
                            if (loc.TryGetProperty("lat", out var latEl) && latEl.TryGetDouble(out var latVal)) plat = latVal;
                            if (loc.TryGetProperty("lng", out var lngEl) && lngEl.TryGetDouble(out var lngVal)) plng = lngVal;
                        }

                        if (!string.IsNullOrWhiteSpace(name) && plat.HasValue && plng.HasValue)
                        {
                            list.Add(new PlaceSuggestionDto
                            {
                                PlaceId = null,
                                Name = name!,
                                Lat = plat,
                                Lng = plng
                            });
                            _logger.LogInformation("Geocoding找到地點: {Name}, 座標: ({Lat}, {Lng})", name, plat, plng);
                        }
                    }

                    if (list.Count > 0)
                    {
                        _logger.LogInformation("Geocoding搜尋成功，返回 {Count} 個結果", list.Count);
                        return list.Take(Math.Max(1, take)).ToList();
                    }
                }
            }

            _logger.LogWarning("PlacesService.SearchBroadAsync 無任何結果返回");
            return Array.Empty<PlaceSuggestionDto>();
        }
    }
}
