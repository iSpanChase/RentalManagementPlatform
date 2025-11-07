using Microsoft.AspNetCore.Authorization;
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

		// 根據已驗證 GuestId 獲取其所有訂單
		[HttpGet("my-bookings/{authenticatedGuestId}")]
		[Authorize(Policy = "Booking.View")]
		public async Task<ActionResult<IEnumerable<BookingDto>>> GetMyBookings(int authenticatedGuestId)
		{
			var bookings = await _bookingService.GetMyBookingsAsync(authenticatedGuestId);
			return Ok(bookings);
		}

		// 根據已驗證 HostId 獲取其所有訂單
		[HttpGet("my-orders/{authenticatedHostId}")]
		[Authorize(Policy = "Booking.ManageBookings")]
		public async Task<ActionResult<IEnumerable<BookingDto>>> GetMyOrders(int authenticatedHostId)
		{
			var bookings = await _bookingService.GetMyOrdersAsync(authenticatedHostId);
			return Ok(bookings);
		}

		// 根據訂單編號獲取單一訂單詳情
		[HttpGet("ordernumber/{orderNumber}")]
		[Authorize(Policy = "Booking.View")]
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
		[Authorize(Policy = "Booking.Create")]
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
		[Authorize(Policy = "Booking.Cancel")]
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