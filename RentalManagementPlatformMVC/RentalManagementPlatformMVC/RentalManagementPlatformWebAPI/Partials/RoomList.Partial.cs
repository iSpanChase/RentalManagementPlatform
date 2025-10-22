namespace RentalManagementPlatformWebAPI.Models
{
	public partial class RoomList
	{
		public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
		public User? Host { get; set; }
	}
}
