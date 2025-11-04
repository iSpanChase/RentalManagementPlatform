
namespace RentalManagementPlatformWebAPI.DTO.RoomList
{
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public string ReviewerName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
