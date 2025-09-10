using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories
{
	public class HostPayoutRepository : IHostPayoutRepository
	{
		private readonly RentalManagementPlatformSqlContext _context;

		public HostPayoutRepository(RentalManagementPlatformSqlContext context)
		{
			_context = context;
		}

		public async Task<(IEnumerable<HostPayout>, int)> GetPagedHostPayoutsAsync(int pageIndex, int pageSize)
		{
			var query = _context.HostPayouts
				.AsNoTracking()
				//.Include(p => p.Host)
				.OrderByDescending(p => p.CreatedAt);

			var totalCount = await query.CountAsync();

			var entities = await query
				.Skip((pageIndex - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (entities, totalCount);
		}

		public async Task<HostPayout?> GetHostPayoutDetailByIdAsync(int hostPayoutId)
		{
			return await _context.HostPayouts
				.AsNoTracking()
				//.Include(p => p.Host)
				.FirstOrDefaultAsync(p => p.PayoutId == hostPayoutId);
		}
	}
}
