namespace RentalManagementPlatformWebAPI.Area.ReportForm.DTO.Recommendation
{
    // 推薦房源DTO
    public class RecommendedRoomDto
    {
        public int RoomId { get; set; }
        public string Title { get; set; } = "";
        public decimal PricePerNight { get; set; }
        public string? mainImageUrl { get; set; }
        public string? AddressLine { get; set; }
        public string? CityName { get; set; }
        public string? DistrictName { get; set; }
        public string? Street { get; set; }
    }
}