namespace RentalManagementPlatformMVC.Models
{
	public partial class Booking
	{
		public User Guest { get; set; }
		public RoomList Room { get; set; }
		public ICollection<Payment> Payments { get; set; } = new List<Payment>();
	}
}
