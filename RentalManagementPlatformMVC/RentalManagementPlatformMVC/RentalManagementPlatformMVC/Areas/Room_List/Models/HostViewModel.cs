using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Room_List.Models
{
    public class HostViewModel
    {
        [Display(Name = "房東")]
        public string? HostName { get; set; }
    }
}
