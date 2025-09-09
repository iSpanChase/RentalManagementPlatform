namespace RentalManagementPlatformMVC.Areas.UserManagement.UserDTOs
{
	public class UserDetailDto
	{
		public int UserId { get; set; }
		public string Username { get; set; } = null!;
		public string Email { get; set; } = null!;
		public string Name { get; set; } = null!;
		public bool? AutoSubscribe { get; set; }
		public string? Gender { get; set; }
		public DateTime? BirthDate { get; set; }
		public string? Phone { get; set; }
		public string? Address { get; set; }
		public int? Point { get; set; }
		public bool? Isverified { get; set; }
		public string? ProfileImageurl { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
