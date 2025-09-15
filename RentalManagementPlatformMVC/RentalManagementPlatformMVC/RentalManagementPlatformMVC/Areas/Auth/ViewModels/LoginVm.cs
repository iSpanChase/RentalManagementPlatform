using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Auth.ViewModels
{
    public class LoginVm
    {
        [Required]
		[Display(Name = "帳號或電子郵件")]
		public string UsernameOrEmail { get; set; } = null!;
        [Required]
		[Display(Name = "密碼")]
		public string Password { get; set; } = null!;
		[Required]
		[Display(Name = "記住我")]
		public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
