using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.UserManagement.ViewModels
{
	public class UserEditVm
	{
		[Required]
		public int UserId { get; set; }

		// 顯示用，不可編輯
		public string Username { get; set; } = null!;

		[Required, MaxLength(512), EmailAddress]
		public string Email { get; set; } = null!;

		[Required, MaxLength(512)]
		public string Name { get; set; } = null!;

		public bool? AutoSubscribe { get; set; }
		public string? Gender { get; set; }
		public DateTime? BirthDate { get; set; }
		[MaxLength(50)] public string? Phone { get; set; }
		[MaxLength(512)] public string? Address { get; set; }
		public int? Point { get; set; }
		public bool? Isverified { get; set; }
		[MaxLength(512)] public string? ProfileImageurl { get; set; }
	}
}
