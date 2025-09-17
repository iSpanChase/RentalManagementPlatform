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
			public const string AssignRoles = "Users.AssignRoles";
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

		public static class Permissions
		{
			public const string View = "Permissions.View";
			public const string Create = "Permissions.Create";
			public const string Edit = "Permissions.Edit";
			public const string Delete = "Permissions.Delete";
			public const string ManagePermissions = "Permissions.ManagePermissions";
		}

		public static class Booking
		{
			public const string View = "Booking.View";
			public const string Create = "Booking.Create";
			public const string Edit = "Booking.Edit";
			public const string Delete = "Booking.Delete";
			public const string ManageBookings = "Booking.ManageBookings";
		}

		public static class FAQ
		{
			public const string View = "FAQ.View";
			public const string Create = "FAQ.Create";
			public const string Edit = "FAQ.Edit";
			public const string Delete = "FAQ.Delete";
			public const string ManageFAQs = "FAQ.ManageFAQs";
		}

		public static class Coupon
		{
			public const string View = "Coupon.View";
			public const string Create = "Coupon.Create";
			public const string Edit = "Coupon.Edit";
			public const string Delete = "Coupon.Delete";
			public const string ManageCoupons = "Coupon.ManageCoupon";
		}

		public static class Payments
		{
			public const string View = "Payments.View";
			public const string Create = "Payments.Create";
			public const string Edit = "Payments.Edit";
			public const string Delete = "Payments.Delete";
			public const string ManagePayments = "Payments.ManagePayments";
		}

		public static class PointRules
		{
			public const string View = "PointRules.View";
			public const string Create = "PointRules.Create";
			public const string Edit = "PointRules.Edit";
			public const string Delete = "PointRules.Delete";
			public const string ManagePointRules = "PointRules.ManagePointRules";
		}

		public static class Favorites
		{
			public const string View = "Favorites.View";
			public const string Create = "Favorites.Create";
			public const string Edit = "Favorites.Edit";
			public const string Delete = "Favorites.Delete";
			public const string ManageFavorites = "Favorites.ManageFavorites";
		}

		public static class RoomList
		{
			public const string View = "RoomList.View";
			public const string Create = "RoomList.Create";
			public const string Edit = "RoomList.Edit";
			public const string Delete = "RoomList.Delete";
			public const string ManageRoomList = "RoomList.ManageRoomList";
		}

		public static class SubscriptionPlan
		{
			public const string View = "SubscriptionPlan.View";
			public const string Create = "SubscriptionPlan.Create";
			public const string Edit = "SubscriptionPlan.Edit";
			public const string Delete = "SubscriptionPlan.Delete";
			public const string ManageSubscriptionPlan = "SubscriptionPlan.ManageSubscriptionPlan";
		}

		public static IEnumerable<string> AllCodes()
		{
			yield return Users.Browse;
			yield return Users.View;
			yield return Users.Create;
			yield return Users.Edit;
			yield return Users.AssignRoles;
			yield return Users.Delete;

			yield return Roles.View;
			yield return Roles.Create;
			yield return Roles.Edit;
			yield return Roles.Delete;
			yield return Roles.ManagePermissions;

			yield return Permissions.View;
			yield return Permissions.Create;
			yield return Permissions.Edit;
			yield return Permissions.Delete;
			yield return Permissions.ManagePermissions;

			yield return Booking.View;
			yield return Booking.Create;
			yield return Booking.Edit;
			yield return Booking.Delete;
			yield return Booking.ManageBookings;

			yield return FAQ.View;
			yield return FAQ.Create;
			yield return FAQ.Edit;
			yield return FAQ.Delete;
			yield return FAQ.ManageFAQs;

			yield return Coupon.View;
			yield return Coupon.Create;
			yield return Coupon.Edit;
			yield return Coupon.Delete;
			yield return Coupon.ManageCoupons;

			yield return Payments.View;
			yield return Payments.Create;
			yield return Payments.Edit;
			yield return Payments.Delete;
			yield return Payments.ManagePayments;

			yield return PointRules.View;
			yield return PointRules.Create;
			yield return PointRules.Edit;
			yield return PointRules.Delete;
			yield return PointRules.ManagePointRules;

			yield return Favorites.View;
			yield return Favorites.Create;
			yield return Favorites.Edit;
			yield return Favorites.Delete;
			yield return Favorites.ManageFavorites;

			yield return RoomList.View;
			yield return RoomList.Create;
			yield return RoomList.Edit;
			yield return RoomList.Delete;
			yield return RoomList.ManageRoomList;

			yield return SubscriptionPlan.View;
			yield return SubscriptionPlan.Create;
			yield return SubscriptionPlan.Edit;
			yield return SubscriptionPlan.Delete;
			yield return SubscriptionPlan.ManageSubscriptionPlan;
		}
	}
}
