namespace RentalManagementPlatformMVC.Models
{
	public partial class Booking
	{
		public ICollection<BookingGuest> BookingGuests { get; set; } = new List<BookingGuest>();
		public User? Guest { get; set; }
		public RoomList? Room { get; set; }
		public Coupon? Coupon { get; set; }
		public ICollection<Payment> Payments { get; set; } = new List<Payment>();
	}
}
