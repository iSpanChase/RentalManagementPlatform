using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatform.Common.Pagination;

namespace RentalManagementPlatformMVC.Repositories
{
	public class PaymentRepository : IPaymentRepository
	{
		private readonly RentalManagementPlatformSqlContext _context;

		public PaymentRepository(RentalManagementPlatformSqlContext context)
		{
			_context = context;
		}

		public async Task<(IEnumerable<Payment>, int)> GetPagedPaymentAsync(int pageIndex, int pageSize)
		{
			var query = _context.Payments
				.AsNoTracking()
				.OrderByDescending(p => p.CreatedAt);

			var totalCount = await query.CountAsync();

			var items = await query
				.Skip((pageIndex - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (items, totalCount);
		}
	}
}
