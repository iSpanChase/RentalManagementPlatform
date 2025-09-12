namespace RentalManagementPlatformMVC.Areas.UserManagement.UserDTOs
{
	public class UpdateUserDto
	{
		public int UserId { get; set; }
		public string Email { get; set; } = null!;
		public string Name { get; set; } = null!;
		public string Gender { get; set; }
		public DateTime BirthDate { get; set; }
		public string? Phone { get; set; }
		public string Address { get; set; }
		public int? Point { get; set; }
		public bool Isverified { get; set; }
		public string? ProfileImageurl { get; set; }
	}
}
