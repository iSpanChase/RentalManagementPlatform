using System.Text.Json.Serialization;

namespace RentalManagementPlatformWebAPI.DTOs
{
    public class PlaceSuggestionDto
    {
        [JsonPropertyName("place_id")]
        public string? PlaceId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("lat")]
        public double? Lat { get; set; }

        [JsonPropertyName("lng")]
        public double? Lng { get; set; }
    }
}

