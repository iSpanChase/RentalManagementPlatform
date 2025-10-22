namespace RentalManagementPlatformWebAPI.Services
{
	public interface IGoogleTokenVerifier
	{
		Task<GoogleProfile?> VerifyAsync(string idToken, string? expectedAudience = null);
	}
	public record GoogleProfile(string Sub, string Email, string? Name, string? Picture, bool EmailVerified);
}
