using RentalManagementPlatformMVC.Areas.Roles.RolesDTOs;

namespace RentalManagementPlatformMVC.Areas.Roles.ViewModels
{
	public class RolesIndexVm
	{
		public required IReadOnlyList<RolesListItemDto> Items { get; init; }
		public required RoleQueryInput Query { get; init; }
		public required PaginationVm Pagination { get; init; }

		public int Total => Pagination.Total;
		public int StartRecord => Total == 0 ? 0 : (Query.Page - 1) * Query.PageSize + 1;
		public int EndRecord => Total == 0 ? 0 : Math.Min(Query.Page * Query.PageSize, Total);
		public int Page => Pagination.Total > 0 ? Pagination.Page : 0;
		public int TotalPages => Pagination.TotalPages;
	}
}
