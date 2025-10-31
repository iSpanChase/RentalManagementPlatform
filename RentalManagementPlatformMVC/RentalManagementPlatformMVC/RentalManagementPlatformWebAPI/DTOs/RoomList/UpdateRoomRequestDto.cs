using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformWebAPI.DTOs
{
    public class UpdateRoomRequestDto
    {
        [Required]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "請輸入標題")]
        [StringLength(100)]
        public string? Title { get; set; }

        [Required(ErrorMessage = "請輸入描述")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "請輸入最大入住人數")]
        [Range(1, 20, ErrorMessage = "人數必須介於 1-20 之間")]
        public int MaxGuests { get; set; }

        [Required(ErrorMessage = "請輸入每晚價格")]
        [Range(1, 100000, ErrorMessage = "價格格式不正確")]
        public decimal PricePerNight { get; set; }
    }
}