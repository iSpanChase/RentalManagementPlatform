namespace RentalManagementPlatformMVC.Models
{
	public partial class HostPayout
	{
		public User Host { get; set; }
		public ICollection<HostPayoutItem> HostPayoutItems { get; set; }
	}
}
