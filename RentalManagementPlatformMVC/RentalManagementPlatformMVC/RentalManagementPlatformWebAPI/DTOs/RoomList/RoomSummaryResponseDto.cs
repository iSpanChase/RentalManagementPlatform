namespace RentalManagementPlatformWebAPI.DTOs
{
    public class RoomSummaryResponseDto
    {
        public int RoomId { get; set; }
        public string? Title { get; set; }
        public string? Status { get; set; }
        public string? HostName { get; set; }
        public string? MainImageUrl { get; set; }
        public bool IsDeleted { get; set; }
    }
}