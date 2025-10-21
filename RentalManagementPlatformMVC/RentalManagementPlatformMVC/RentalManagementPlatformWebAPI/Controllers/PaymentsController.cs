using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services.Interface;

namespace RentalManagementPlatformWebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PaymentsController : ControllerBase
	{
		private readonly IPaymentsService _paymentsService;

		public PaymentsController(IPaymentsService paymentsService)
		{
			_paymentsService = paymentsService;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<PaymentsDto>>> GetAllPaymentsAsync()
		{
			var payments = await _paymentsService.GetAllPaymentsAsync();
			return Ok(payments);
		}

		[HttpPost]
		public async Task<ActionResult<PaymentsDto>> CreatePaymentAsync([FromBody] CreatePaymentDto dto)
		{
			var createdPayment = await _paymentsService.CreatePaymentAsync(dto);

			return CreatedAtAction(
				nameof(GetPaymentById), 
				new { paymentId = createdPayment.PaymentId }, 
				createdPayment
			);
		}

		[HttpGet]
		[Route("{paymentId:int}")]
		public async Task<ActionResult<PaymentsDto>> GetPaymentById(int paymentId)
		{
			var payment = await _paymentsService.GetPaymentByIdAsync(paymentId);

			if (payment == null)
			{
				return NotFound();
			}

			return Ok(payment);
		}

		[HttpGet]
		[Route("host/{hostId:int}")]
		public async Task<ActionResult<IEnumerable<PaymentsDto>>> GetPaymentsByHostIdAsync(int hostId)
		{
			var payments = await _paymentsService.GetPaymentsByHostIdAsync(hostId);

			if (payments == null || !payments.Any())
			{
				return NotFound();
			}

			return Ok(payments);
		}
	}
}
