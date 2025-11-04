namespace RentalManagementPlatformWebAPI.Area.ReportForm.DTO.Recommendation
{
    // 推薦房源DTO
    public class RecommendedRoomDto
    {
        public int RoomId { get; set; }
        public string Title { get; set; } = "";
        public decimal PricePerNight { get; set; }
        public string? ImageUrl { get; set; }
        public string? Address {  get; set; }
    }
}