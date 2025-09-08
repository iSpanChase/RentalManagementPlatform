namespace RentalManagementPlatformMVC.Models
{
	public partial class Payment
	{
		public Booking Booking { get; set; }
		public PaymentTransaction PaymentTransaction { get; set; }
	}
}