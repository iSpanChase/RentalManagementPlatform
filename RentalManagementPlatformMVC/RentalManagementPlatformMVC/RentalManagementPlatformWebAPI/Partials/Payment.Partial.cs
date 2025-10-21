namespace RentalManagementPlatformWebAPI.Models
{
	public partial class Payment
	{
		public Booking? Booking { get; set; }
		public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
	}
}