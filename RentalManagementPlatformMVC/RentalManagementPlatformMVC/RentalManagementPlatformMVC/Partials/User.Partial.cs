namespace RentalManagementPlatformMVC.Models
{
	public partial class User
	{
		public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
		public ICollection<RoomList> RoomLists { get; set; } = new List<RoomList>();
		public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
	}
}