using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOs.Bookings;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Services.Interfaces;

namespace RentalManagementPlatformWebAPI.Area.Bookings.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BookingsController : ControllerBase
	{
		private readonly IBookingService _bookingService;

		public BookingsController(IBookingService bookingService, RentalManagementPlatformSqlContext context)
		{
			_bookingService = bookingService;
		}

		// 取得所有訂單(測試用)
		[HttpGet]
		public async Task<ActionResult<IEnumerable<BookingDto>>> GetAllBookingsAsync()
		{
			var bookings = await _bookingService.GetAllBookingsAsync();
			return Ok(bookings);
		}

		// [開發用] 根據使用者ID獲取其所有訂單
		// 未來與登入功能整合後，應改為從 HttpContext 的 Claims 獲取 userId，並加上 [Authorize]
		[HttpGet("user/{guestId}")]
		public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookingsByUserId(int guestId)
		{
			var bookings = await _bookingService.GetBookingsByUserAsync(guestId);
			return Ok(bookings);
		}

		// 建立訂單並產生綠界付款表單
		[HttpPost("create-and-pay")]
		public async Task<ActionResult<CreateOrderAndPayResponseDto>> CreateBookingWithPaymentAsync(
			[FromBody] CreateBookingWithPaymentDto dto)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var result = await _bookingService.CreateBookingWithPaymentAsync(dto);

				return Ok(result);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new
				{
					message = "建立訂單失敗",
					error = ex.Message,
					innerException = ex.InnerException?.Message
				});
			}
		}
	}
}