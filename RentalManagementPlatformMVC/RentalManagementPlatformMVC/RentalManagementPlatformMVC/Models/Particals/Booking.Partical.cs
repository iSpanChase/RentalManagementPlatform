namespace RentalManagementPlatformMVC.Models
{
	public partial class Booking
	{
		public virtual ICollection<BookingGuest> BookingGuests { get; set; } = new List<BookingGuest>();
	}
}
