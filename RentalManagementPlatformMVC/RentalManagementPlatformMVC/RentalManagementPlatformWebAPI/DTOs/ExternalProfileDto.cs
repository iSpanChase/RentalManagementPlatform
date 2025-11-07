namespace RentalManagementPlatformWebAPI.DTOs
{
	public class ExternalProfileDto
	{
		public string Provider { get; set; } = "";         // "Google" / "LINE"
		public string ProviderUserId { get; set; } = "";   // Google sub 或 LINE userId
		public string? Email { get; set; }
		public string? DisplayName { get; set; }
		public string? PictureUrl { get; set; }
	}
}
