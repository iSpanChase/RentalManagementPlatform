using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Permissions.ViewModels
{
	public class PermissionFormVm
	{
		public int? PermissionId { get; set; }

		[Required, StringLength(100)]
		public string PermCode { get; set; } = default!;

		[Required, StringLength(200)]
		public string PermName { get; set; } = default!;

		[Required, StringLength(50)]
		public string Module { get; set; } = default!;

		[Required, StringLength(50)]
		public string Action { get; set; } = default!;

		[StringLength(500)]
		public string? Description { get; set; }
	}
}
