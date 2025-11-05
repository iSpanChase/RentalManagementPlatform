namespace RentalManagementPlatformWebAPI.DTOs
{
	public class CompleteExternalDto
	{
		public string Email { get; set; } = default!;
		public string? Phone { get; set; }
		public string? DisplayName { get; set; }
	}
}
