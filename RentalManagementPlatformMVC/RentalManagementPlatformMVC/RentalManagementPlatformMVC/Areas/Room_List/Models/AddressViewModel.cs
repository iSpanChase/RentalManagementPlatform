using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Room_List.Models
{
    public class AddressViewModel
    {
        [Display(Name = "地址")]
        public string? FullAddress { get; set; }
    }
}
