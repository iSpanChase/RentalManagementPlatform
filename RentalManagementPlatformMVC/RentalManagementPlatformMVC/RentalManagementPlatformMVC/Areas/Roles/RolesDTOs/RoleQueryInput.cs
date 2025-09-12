namespace RentalManagementPlatformMVC.Areas.Roles.RolesDTOs
{
	public class RoleQueryInput
	{
		public string? Keyword { get; set; }      // 搜 RoleName/RoleCode/Description
		public string? RoleName { get; set; }    // 名稱關鍵字
		public string? RoleCode { get; set; }    // 代碼關鍵字
		public string? Description { get; set; } // 描述關鍵字
		public string? SortBy { get; set; } = "RoleName"; // RoleName|RoleCode|UserCount|PermCount
		public bool Desc { get; set; } = false;
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}
}
