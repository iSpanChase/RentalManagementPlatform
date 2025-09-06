using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.Repositories;
using RentalManagementPlatformMVC.DTOs;

namespace RentalManagementPlatformMVC.Services
{
	public class PaymentService : IPaymentService
	{
		private readonly IPaymentRepository _paymentRepository;
		
		public PaymentService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository;
		}

		public async Task<PagedResult<PaymentDto>> GetPagedPaymentsAsync(int pageIndex, int pageSize)
		{
			var (entities, totalCount) = await _paymentRepository.GetPagedPaymentAsync(pageIndex, pageSize);

			var paymentDtos = entities.Select(p => new PaymentDto
			{
				PaymentId = p.PaymentId,
				OrderNumberSnapshot = p.OrderNumberSnapshot,
				Amount = p.Amount,
				PaymentRef = p.PaymentRef,
				Method = p.Method,
				PaidAt = p.PaidAt,
				Status = p.Status,
				CreatedAt = p.CreatedAt,
			}).ToList();

			return new PagedResult<PaymentDto>
			{
				Items = paymentDtos,
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalCount = totalCount
			};
		}
	}
}
