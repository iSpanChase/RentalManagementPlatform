namespace RentalManagementPlatformWebAPI.DTOs
{
	public record LoginRequestDto(string Email, string Password);
	public record GoogleLoginDto(string IdToken);
	public record TokenRefreshDto(string RefreshToken);
	public class LoginResponseDto
	{
		public string AccessToken { get; set; } = null!;
		public DateTime ExpiresAt { get; set; }
		public string RefreshToken { get; set; } = null!;
		public UserProfileDto Profile { get; set; } = null!;
		public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
		public IEnumerable<string> Permissions { get; set; } = Array.Empty<string>();
	}
}
