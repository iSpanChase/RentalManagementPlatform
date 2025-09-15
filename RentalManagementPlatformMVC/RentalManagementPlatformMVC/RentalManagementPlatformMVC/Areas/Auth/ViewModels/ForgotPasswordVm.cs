using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Auth.ViewModels
{
    public class ForgotPasswordVm
    {
        [Required, EmailAddress] public string Email { get; set; } = null!;
    }
}
