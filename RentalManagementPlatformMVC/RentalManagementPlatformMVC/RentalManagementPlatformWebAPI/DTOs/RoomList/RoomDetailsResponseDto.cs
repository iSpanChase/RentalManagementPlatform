using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.DTOs
{
    public class RoomDetailsResponseDto
    {
        public int RoomId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int MaxGuests { get; set; }
        public decimal PricePerNight { get; set; }
        public string? Status { get; set; }
        public bool IsDeleted { get; set; }
        public AddressDto? Address { get; set; } // Changed to AddressDto
        public HostDto? Host { get; set; } // Changed to HostDto
        public List<string>? PhotoUrls { get; set; }
        public int? HostId { get; set; }
        public string? CityName { get; set; }
        public int? DistrictId { get; set; }
        public string? DistrictName { get; set; }
        public string? AddressLine { get; set; }
        public GeoLocation? Geo { get; set; }
        public double? RatingAvg { get; set; }
        public int? ReviewsCount { get; set; }
        public string? CoverBucket { get; set; }
        public string? CoverObjectKey { get; set; }
        public string? CoverContentType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> Amenities { get; set; } = new();
    }
}