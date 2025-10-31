using System;
using System.Text.Json.Serialization;

namespace RentalManagementPlatformWebAPI.DTOs
{
    public class RoomListIndexDto
    {
        [JsonPropertyName("room_id")]
        public int RoomId { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("price_per_night")]
        public decimal? PricePerNight { get; set; }

        [JsonPropertyName("max_guests")]
        public int? MaxGuests { get; set; }

        [JsonPropertyName("host_id")]
        public int? HostId { get; set; }

        [JsonPropertyName("city_name")]
        public string? CityName { get; set; }

        [JsonPropertyName("district_id")]
        public int? DistrictId { get; set; }

        [JsonPropertyName("district_name")]
        public string? DistrictName { get; set; }

        [JsonPropertyName("address_line")]
        public string? AddressLine { get; set; }

        [JsonPropertyName("_geo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public GeoLocation? Geo { get; set; }

        [JsonPropertyName("rating_avg")]
        public double? RatingAvg { get; set; }

        [JsonPropertyName("reviews_count")]
        public int? ReviewsCount { get; set; }
        [JsonPropertyName("cover_bucket")]
        public string? CoverBucket { get; set; }

        [JsonPropertyName("cover_object_key")]
        public string? CoverObjectKey { get; set; }

        [JsonPropertyName("cover_content_type")]
        public string? CoverContentType { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}