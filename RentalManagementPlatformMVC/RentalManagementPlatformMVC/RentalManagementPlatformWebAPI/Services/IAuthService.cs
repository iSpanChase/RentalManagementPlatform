using RentalManagementPlatformWebAPI.DTOs;

namespace RentalManagementPlatformWebAPI.Services
{
	public interface IAuthService
	{
		Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
		Task<LoginResponseDto> GoogleLoginAsync(string idToken);
		Task<LoginResponseDto> RefreshAsync(string refreshToken);
		Task RevokeAllAsync(int userId);
	}
}
