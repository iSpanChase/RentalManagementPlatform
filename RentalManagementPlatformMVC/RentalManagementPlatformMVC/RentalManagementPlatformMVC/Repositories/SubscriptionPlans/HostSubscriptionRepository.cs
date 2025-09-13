using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories.SubscriptionPlans
{
	public class HostSubscriptionRepository : IHostSubscriptionRepository
	{
		private readonly RentalManagementPlatformSqlContext _context;

		public HostSubscriptionRepository(RentalManagementPlatformSqlContext context)
		{
			_context = context;
		}

		public async Task<(IEnumerable<HostSubscription>, int)> GetPagedHostSubscriptionsAsync(int pageIndex, int pageSize)
		{
			var query = _context.HostSubscriptions
				.AsNoTracking()
				.Include(hs => hs.Host)
				.Include(hs => hs.Plan)
				.OrderByDescending(hs => hs.CreatedAt);

			var totalCount = await query.CountAsync();

			var entities = await query
				.Skip((pageIndex - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (entities, totalCount);
		}
	}
}
