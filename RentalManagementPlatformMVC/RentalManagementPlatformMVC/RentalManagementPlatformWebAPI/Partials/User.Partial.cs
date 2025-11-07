namespace RentalManagementPlatformWebAPI.Models
{
	public partial class User
	{
		public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
		public ICollection<RoomList> RoomLists { get; set; } = new List<RoomList>();
		public ICollection<HostPayout> HostPayouts { get; set; } = new List<HostPayout>();
		public ICollection<HostSubscription> HostSubscriptions { get; set; } = new List<HostSubscription>();
		public ICollection<PointLedger> PointLedgers { get; set; } = new List<PointLedger>();
		public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
		public ICollection<EmailVerification> EmailVerifications { get; set; } = new List<EmailVerification>();
		public virtual ICollection<ExternalLogin> ExternalLogins { get; set; } = new List<ExternalLogin>();

	}
}