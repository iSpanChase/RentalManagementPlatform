using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;

namespace RentalManagementPlatformWebAPI.Repositories.Payments
{
	public class PaymentsRepository : IPaymentsRepository
	{
		private readonly RentalManagementPlatformSqlContext _context;

		public PaymentsRepository(RentalManagementPlatformSqlContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
		{
			return await _context.Payments
				.AsNoTracking()
				.Include(p => p.Booking)
				.Include(p => p.PaymentTransactions)
				.ToListAsync();
		}
	}
}
