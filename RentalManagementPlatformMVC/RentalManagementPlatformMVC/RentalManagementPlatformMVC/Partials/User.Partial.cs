namespace RentalManagementPlatformMVC.Models
{
	{
		public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
		public ICollection<RoomList> RoomLists { get; set; } = new List<RoomList>();
	}
}