namespace RentalManagementPlatformMVC.Areas.Roles.RolesDTOs
{
	public class AssignPermissionsQueryInput
	{
		public int RoleId { get; set; }

		public string? Keyword { get; set; } // Module/Action/Name/Code
		public string SortBy { get; set; } = "Code"; // Module|Action|Name|Code
		public bool Desc { get; set; } = false;

		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}
}
