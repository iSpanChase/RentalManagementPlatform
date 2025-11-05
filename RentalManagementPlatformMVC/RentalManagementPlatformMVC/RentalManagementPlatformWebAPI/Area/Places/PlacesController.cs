using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RentalManagementPlatformWebAPI.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlacesController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpFactory;

        public PlacesController(IConfiguration config, IHttpClientFactory httpFactory)
        {
            _config = config;
            _httpFactory = httpFactory;
        }

        // GET: api/Places/search?q=台北101
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PlaceSuggestionDto>>> Search(string q)
        {
            q = (q ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(Array.Empty<PlaceSuggestionDto>());
            }

            var apiKey = _config["Google:ApiKey"]
                         ?? Environment.GetEnvironmentVariable("GOOGLE_MAPS_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return StatusCode(500, "Google API key not configured");
            }

            // Favor Taipei attractions; bias location and radius 30km
            var lat = 25.033964; // Taipei 101 approx
            var lng = 121.564468;
            var radius = 30000; // 30km

            var url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={Uri.EscapeDataString(q)}&type=tourist_attraction&language=zh-TW&location={lat},{lng}&radius={radius}&key={apiKey}";

            using var client = _httpFactory.CreateClient();
            using var resp = await client.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            await using var stream = await resp.Content.ReadAsStreamAsync();

            using var doc = await JsonDocument.ParseAsync(stream);
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
                            Name = name,
                            Lat = plat,
                            Lng = plng
                        });
                    }
                }

                return Ok(list.Take(10));
            }

            return Ok(Array.Empty<PlaceSuggestionDto>());
        }
    }
}

