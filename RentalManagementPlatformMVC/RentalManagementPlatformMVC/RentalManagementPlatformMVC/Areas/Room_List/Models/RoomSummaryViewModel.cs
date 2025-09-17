using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Room_List.Models
{
    public class RoomSummaryViewModel
    
    
    {   [Display(Name = "房源ID")]
        public int RoomId { get; set; }

        [Display(Name = "標題")]
        public string? Title { get; set; }

        [Display(Name = "狀態")]
        public string? Status { get; set; }

        [Display(Name = "房東")]
        public string? HostName { get; set; }
        
        public string? MainImageUrl { get; set; }

        public bool IsDeleted { get; set; }
    }
}