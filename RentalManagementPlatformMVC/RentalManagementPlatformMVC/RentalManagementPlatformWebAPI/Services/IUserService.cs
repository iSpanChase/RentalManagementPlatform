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
		Task<int?> GetUserIdByEmailAsync(string email);
		Task<UserProfileDto?> GetProfileByIdAsync(int userId);
        Task<UserProfileDto?> FAQGetProfileByUsername(string username);

		Task<UserProfileDto> FindOrCreateFromExternalAsync(ExternalProfileDto dto);
	}
}
