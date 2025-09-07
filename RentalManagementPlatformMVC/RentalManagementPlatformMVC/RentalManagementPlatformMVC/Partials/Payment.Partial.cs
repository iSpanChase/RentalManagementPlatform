namespace RentalManagementPlatformMVC.Partials
{
	public partial class Payment
	{
		public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
	}
}
