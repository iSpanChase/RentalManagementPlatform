using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Roles.ViewModels
{
	public class RoleFormVm
	{
		public int? RoleId { get; set; }  // Create: null, Edit: 有值

		[Required, MaxLength(50)]
		public string RoleCode { get; set; } = null!;

		[Required, MaxLength(100)]
		public string RoleName { get; set; } = null!;

		[MaxLength(500)]
		public string? Description { get; set; }

		// 顯示用
		public string Title => RoleId.HasValue ? "Edit Role" : "Create Role";
	}
}
