namespace RentalManagementPlatformMVC.Areas.Permissions.Models
{
	public static class AppPermissions
	{
		public static class Users
		{
			public const string View = "Users.View";
			public const string Browse = "Users.Browse";
			public const string Create = "Users.Create";
			public const string Edit = "Users.Edit";
			public const string Delete = "Users.Delete";
		}

		public static class Roles
		{
			public const string View = "Roles.View";
			public const string Create = "Roles.Create";
			public const string Edit = "Roles.Edit";
			public const string Delete = "Roles.Delete";
			public const string ManagePermissions = "Roles.ManagePermissions";
		}

		public static IEnumerable<(string code, string name, string module, string action, string? desc)> All()
		{
			yield return (Users.View, "檢視使用者", "Users", "View", null);
			yield return (Users.Browse, "瀏覽使用者清單", "Users", "Browse", null);
			yield return (Users.Create, "新增使用者", "Users", "Create", null);
			yield return (Users.Edit, "編輯使用者", "Users", "Edit", null);
			yield return (Users.Delete, "刪除使用者", "Users", "Delete", null);

			yield return (Roles.View, "檢視角色", "Roles", "View", null);
			yield return (Roles.Create, "新增角色", "Roles", "Create", null);
			yield return (Roles.Edit, "編輯角色", "Roles", "Edit", null);
			yield return (Roles.Delete, "刪除角色", "Roles", "Delete", null);
			yield return (Roles.ManagePermissions, "管理角色權限", "Roles", "ManagePermissions", null);
		}
	}
}
