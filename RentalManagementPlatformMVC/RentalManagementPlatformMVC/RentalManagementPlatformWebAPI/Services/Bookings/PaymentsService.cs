using AutoMapper;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.DTOs.Bookings;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;
using RentalManagementPlatformWebAPI.Services.Interfaces;

namespace RentalManagementPlatformWebAPI.Services.Bookings
{
	public class PaymentsService : IPaymentsService
	{
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly IMapper _mapper;

		public PaymentsService(IPaymentsRepository paymentsRepository, IMapper mapper)
		{
			_paymentsRepository = paymentsRepository;
			_mapper = mapper;
		}

		public async Task<IEnumerable<PaymentsDto>> GetAllPaymentsAsync()
		{
			var payments = await _paymentsRepository.GetAllPaymentsAsync();
			return _mapper.Map<IEnumerable<PaymentsDto>>(payments);
		}

		public async Task<PaymentsDto> CreatePaymentAsync(CreatePaymentDto dto)
		{
			var payment = new Payment
			{
				BookingId = dto.BookingId,
				Amount = dto.Amount,
				Method = dto.Method,
				PaidAt = dto.PaidAt,
				PaymentRef = dto.PaymentRef,
				OrderNumberSnapshot = dto.OrderNumberSnapshot,
				Status = dto.Status,
				CreatedAt = DateTime.Now
			};

			await _paymentsRepository.CreatePaymentAsync(payment);

			return new PaymentsDto
			{
				PaymentId = payment.PaymentId,
				BookingId = payment.BookingId,
				Amount = payment.Amount,
				Method = payment.Method,
				PaidAt = payment.PaidAt,
				PaymentRef = payment.PaymentRef,
				OrderNumberSnapshot = payment.OrderNumberSnapshot,
				Status = payment.Status,
				CreatedAt = payment.CreatedAt
			};
		}

		public async Task<PaymentsDto?> GetPaymentByIdAsync(int paymentId)
		{
			var payment = await _paymentsRepository.GetPaymentByIdAsync(paymentId);

			if (payment == null) 
			{
				return null;
			}

			return _mapper.Map<PaymentsDto>(payment);
		}

		public async Task<IEnumerable<PaymentsDto>> GetPaymentsByHostIdAsync(int hostId)
		{
			var payments = await _paymentsRepository.GetPaymentsByHostIdAsync(hostId);

			if (payments == null || !payments.Any())
			{
				return null;
			}

			return _mapper.Map<IEnumerable<PaymentsDto>>(payments);
		}
	}
}
