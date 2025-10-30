using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.DTOs
{
    public class RoomSummaryResponseDto
    {
        public int RoomId { get; set; }
        public string? Title { get; set; }
        public string? Status { get; set; }
        public int? HostId { get; set; }
        public string? HostName { get; set; }
        public string? Description { get; set; }
        public GeoLocation? Geo { get; set; }
        public string? MainImageUrl { get; set; }
        public List<string> PhotoUrls { get; set; } = new List<string>();
        public decimal PricePerNight { get; set; }
        public decimal RatingAvg { get; set; }
        public int ReviewsCount { get; set; }
        public string? CityName { get; set; }
        public string? DistrictName { get; set; }
        public string? AddressLine { get; set; }
        public int MaxGuests { get; set; }
        public bool IsDeleted { get; set; }
    }
}
