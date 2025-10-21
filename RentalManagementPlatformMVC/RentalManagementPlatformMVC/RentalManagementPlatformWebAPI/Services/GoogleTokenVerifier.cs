using Google.Apis.Auth;

namespace RentalManagementPlatformWebAPI.Services
{
	public class GoogleTokenVerifier : IGoogleTokenVerifier
	{
		public async Task<GoogleProfile?> VerifyAsync(string idToken, string? expectedAudience = null)
		{
			var settings = new GoogleJsonWebSignature.ValidationSettings
			{
				Audience = expectedAudience is null ? null : new[] { expectedAudience }
			};

			try
			{
				var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
				return new GoogleProfile(
					Sub: payload.Subject,
					Email: payload.Email,
					Name: payload.Name,
					Picture: payload.Picture,
					EmailVerified: payload.EmailVerified
				);
			}
			catch
			{
				return null;
			}
		}
	}
}
