namespace RentalManagementPlatformMVC.Models
{
	public partial class Booking
	{
		public User Guest { get; set; }
		public RoomList Room { get; set; }
	}
}
