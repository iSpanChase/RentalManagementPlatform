using RentalManagementPlatformMVC.Areas.Auth.ViewModels;

namespace RentalManagementPlatformMVC.Areas.Auth.Services
{
	public interface IAuthService
	{
		Task<bool> SignInAsync(HttpContext http, string usernameOrEmail, string password, bool rememberMe);
		Task SignOutAsync(HttpContext http);
		//Task<int> RegisterAsync(string username, string email, string name, string password);
		Task<int> RegisterAsync(RegisterVm vm);
		Task<string> CreatePasswordResetAsync(string email);                 // 產生重設連結（回傳給寄信用）
		Task<bool> ResetPasswordAsync(int userId, string token, string newPassword);
		// 依 userId 重新簽發 Cookie（刷新 Claims）
		Task RefreshClaimsAsync(HttpContext http, int userId);
	}
}
