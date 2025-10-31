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

		// [開發用] 根據 HostId 獲取其所有訂單
		[HttpGet("host/{hostId}")]
		public async Task<ActionResult<IEnumerable<BookingDto>>> GetOrdersByHostId(int hostId)
		{
			var bookings = await _bookingService.GetOrdersByHostIdAsync(hostId);
			return Ok(bookings);
		}

		// 根據訂單編號獲取單一訂單詳情
		[HttpGet("ordernumber/{orderNumber}")]
		public async Task<ActionResult<BookingDto>> GetBookingByOrderNumber(string orderNumber)
		{
			var booking = await _bookingService.GetBookingByOrderNumberAsync(orderNumber);
			if (booking == null)
			{
				return NotFound($"找不到訂單編號為 {orderNumber} 的訂單");
			}
			return Ok(booking);
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

		// 根據 BookingId 取消訂單
		[HttpPut("cancel/{bookingId}")]
		public async Task<IActionResult> CancelBookingAsync(int bookingId)
		{
			var result = await _bookingService.CancelBookingByIdAsync(bookingId);

			var apiResponse = new
			{
				success = true,
				message = result != null ? "訂單取消成功" : "找不到訂單或無法取消",
				booking = result
			};

			return Ok(apiResponse);
		}
	}
}