namespace RentalManagementPlatformWebAPI.Services
{
	public record JwtPair(string AccessToken, DateTime ExpiresAt, string RefreshToken);
	public interface IJwtTokenService
	{
		JwtPair Create(int userId, string email, string fullName, IEnumerable<string> roles, IEnumerable<string> permissions);
	}
}
