namespace RentalManagementPlatformMVC.Models
{
	public partial class RoomList
	{
		public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
	}
}
