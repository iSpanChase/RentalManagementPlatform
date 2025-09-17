using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Auth.ViewModels
{
    public class ResetPasswordVm
    {
        public int UserId { get; set; }
        public string Token { get; set; } = null!;
        [Required, MinLength(6)] public string Password { get; set; } = null!;
    }
}
