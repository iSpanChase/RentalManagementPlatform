namespace RentalManagementPlatformMVC.Areas.Permissions.PermissionsDTOs
{
	public class PermissionQueryInput
	{
		public string? Keyword { get; set; }
		public string? Module { get; set; }
		public string? Action { get; set; }
		/// <summary>code | name | module | action | created</summary>
		public string? SortBy { get; set; }
		public bool Desc { get; set; }
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 20;
	}
}
