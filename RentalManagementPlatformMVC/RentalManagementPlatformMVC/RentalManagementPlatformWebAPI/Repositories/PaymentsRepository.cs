using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Interface;

namespace RentalManagementPlatformWebAPI.Repositories
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

		public async Task CreatePaymentAsync(Payment payment)
		{
			_context.Payments.Add(payment);
			await _context.SaveChangesAsync();
		}

		public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
		{
			return await _context.Payments
				.Include(p => p.Booking)
				.Include(p => p.PaymentTransactions)
				.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
		}

		public async Task<IEnumerable<Payment>> GetPaymentsByHostIdAsync(int hostId)
		{
			return await _context.Payments
				.Include(p => p.Booking)
				.ThenInclude(b => b.Room)
				.Where(p => p.Booking.Room.HostId == hostId)
				.Include(p => p.PaymentTransactions)
				.ToListAsync();
		}
	}
}
