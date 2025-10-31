
namespace RentalManagementPlatformWebAPI.DTO.RoomList
{
    public class CreateReviewDto
    {
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}
