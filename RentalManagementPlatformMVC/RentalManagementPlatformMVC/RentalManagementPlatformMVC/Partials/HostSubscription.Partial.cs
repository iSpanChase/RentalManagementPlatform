using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Models
{
	public partial class HostSubscription
	{
		public SubscriptionPlan Plan { get; set; }
		public User Host { get; set; }
		public ICollection<SubscriptionBillingLog> SubscriptionBillingLogs { get; set; } = new List<SubscriptionBillingLog>();
	}
}
