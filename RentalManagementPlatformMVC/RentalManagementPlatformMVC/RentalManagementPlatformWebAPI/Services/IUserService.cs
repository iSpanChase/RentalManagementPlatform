using RentalManagementPlatformWebAPI.DTOs;
using System.Security.Claims;

namespace RentalManagementPlatformWebAPI.Services
{
	public interface IUserService
	{
		Task<UserProfileDto> RegisterAsync(RegistrationRequestDto dto);
		Task<UserProfileDto> GetProfileAsync(ClaimsPrincipal principal);
		Task<UserProfileDto> UpdateProfileAsync(ClaimsPrincipal principal, UpdateProfileDto dto);
		Task<string> UploadAvatarAsync(ClaimsPrincipal principal, IFormFile file);
	}
}
