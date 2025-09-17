using Meilisearch;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentalManagementPlatformMVC.Areas.Roles.RolesDTOs;

namespace RentalManagementPlatformMVC.Areas.Roles.ViewModels
{
	public class AssignUsersVm
	{
		public int RoleId { get; set; }
		public string RoleName { get; set; } = null!;

		// 畫面選到的使用者（多選）
		public List<int> SelectedUserIds { get; set; } = new();
		// 下拉或左右清單資料來源
		public List<SelectListItem> AllUsers { get; set; } = new();
	}
}
