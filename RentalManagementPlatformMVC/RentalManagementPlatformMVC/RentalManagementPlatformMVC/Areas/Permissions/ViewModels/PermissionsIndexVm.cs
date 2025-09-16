using RentalManagementPlatformMVC.Areas.Permissions.PermissionsDTOs;
using RentalManagementPlatformMVC.Areas.Roles.ViewModels;

namespace RentalManagementPlatformMVC.Areas.Permissions.ViewModels
{
	public class PermissionsIndexVm
	{
		public List<PermissionListItemDto> Items { get; set; } = new();
		public PermissionQueryInput Query { get; set; } = new();
		public PaginationVm Pagination { get; set; } = new();
		public List<string> Modules { get; set; } = new();
		public List<string> Actions { get; set; } = new();
		public int Total => Pagination.Total;
		public int StartRecord => Total == 0 ? 0 : (Query.Page - 1) * Query.PageSize + 1;
		public int EndRecord => Total == 0 ? 0 : Math.Min(Query.Page * Query.PageSize, Total);
		public int Page => Pagination.Total > 0 ? Pagination.Page : 0;
		public int TotalPages => Pagination.TotalPages;
	}
}
