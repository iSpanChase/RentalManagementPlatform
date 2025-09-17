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

		public static IEnumerable<string> AllCodes()
		{
			yield return Users.Browse;
			yield return Users.View;
			yield return Users.Create;
			yield return Users.Edit;
			yield return Users.Delete;

			yield return Roles.View;
			yield return Roles.Create;
			yield return Roles.Edit;
			yield return Roles.Delete;
			yield return Roles.ManagePermissions;
		}
	}
}
