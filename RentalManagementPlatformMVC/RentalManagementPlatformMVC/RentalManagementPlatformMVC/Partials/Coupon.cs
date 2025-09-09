namespace RentalManagementPlatformMVC.Models
{
	public partial class Coupon
	{
		public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
	}
}
