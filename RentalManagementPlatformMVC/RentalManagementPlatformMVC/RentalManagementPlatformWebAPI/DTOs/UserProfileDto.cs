namespace RentalManagementPlatformWebAPI.DTOs
{
	public class UserProfileDto
	{
		public int UserId { get; set; }
		public string Email { get; set; } = null!;
		public string Name { get; set; } = null!;
		public string Username { get; set; } = null!;
		public string Phone { get; set; } = "";
		public string ProfileImageurl { get; set; } = "";
	}
}
