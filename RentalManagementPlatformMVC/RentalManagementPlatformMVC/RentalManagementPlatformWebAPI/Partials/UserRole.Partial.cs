namespace RentalManagementPlatformWebAPI.Models
{
	public partial class UserRole
	{
		// 新增：導航
		public virtual User User { get; set; } = null!;
		public virtual Role Role { get; set; } = null!;
	}
}
