namespace RentalManagementPlatformMVC.Models
{
	public partial class User
	{
		public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
	}
}
