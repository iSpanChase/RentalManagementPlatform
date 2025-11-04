namespace RentalManagementPlatformWebAPI.DTOs
{
	public class UserProfileDto
	{
		public int UserId { get; set; }
		public string Username { get; set; } = "";
		public string Email { get; set; } = "";
		public string Name { get; set; } = "";

		public string Gender { get; set; } = "";
		public DateTime BirthDate { get; set; }   // 前端以 yyyy-MM-dd 顯示/編輯
		public string Phone { get; set; } = "";
		public string Address { get; set; } = "";
		public int? Point { get; set; }

		public string? ProfileImageUrl { get; set; }
		public bool IsVerified { get; set; }      // 對應 USER.isverified
	}
}
