namespace RentalManagementPlatformMVC.Models
{
	public partial class SubscriptionPlan
	{
		public ICollection<HostSubscription> HostSubscriptions { get; set; } = new List<HostSubscription>();
	}
}
