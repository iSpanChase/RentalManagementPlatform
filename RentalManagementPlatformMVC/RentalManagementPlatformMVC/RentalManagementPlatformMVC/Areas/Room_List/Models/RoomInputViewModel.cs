using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Areas.Room_List.Models
{
    public class RoomInputViewModel
    {
        public int? RoomId { get; set; }

        [Required(ErrorMessage = "請輸入標題")]
        [StringLength(100)]
        [Display(Name = "標題")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "請輸入描述")]
        [Display(Name = "描述")]
        public string? Description { get; set; }

        [Display(Name = "最大入住人數")]
        [Required(ErrorMessage = "請輸入最大入住人數")]
        [Range(1, 20, ErrorMessage = "人數必須介於 1-20 之間")]
        public int MaxGuests { get; set; }

        [Display(Name = "每晚價格")]
        [Required(ErrorMessage = "請輸入每晚價格")]
        [Range(1, 100000, ErrorMessage = "價格格式不正確")]
        public decimal PricePerNight { get; set; }

        // You can add properties for Address fields here
        // For example:
        // [Required]
        // public string Street { get; set; }
        // [Required]
        // public string City { get; set; }

        public List<SelectListItem>? StatusOptions { get; set; }
    }
}