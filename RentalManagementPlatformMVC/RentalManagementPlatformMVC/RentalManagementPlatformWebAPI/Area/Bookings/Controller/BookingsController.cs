using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.DTOs.Bookings;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Services.Interfaces;

namespace RentalManagementPlatformWebAPI.Area.Bookings.Controller
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
		public async Task<ActionResult<IEnumerable<Booking>>> GetAllBookingsAsync()
		{
			var bookings = await _bookingService.GetAllBookingsAsync();
			return Ok(bookings);
		}

		/// <summary>
		/// 建立訂單並產生綠界付款表單
		/// </summary>
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

		[HttpGet("{bookingId:int}")]
		public async Task<ActionResult<Booking>> GetBookingById(int bookingId)
		{
			var booking = await _bookingService.GetBookingByIdAsync(bookingId);

			if (booking == null)
			{
				return NotFound();
			}

			return booking == null ? NotFound() : Ok(booking);
		}

		[HttpGet("~/api/users/{userId:int}/bookings")]
		public async Task<ActionResult<IEnumerable<Booking>>> GetBookingByUserAsync(int userId)
		{
			var bookings = await _bookingService.GetBookingsByUserAsync(userId);

			if (bookings == null || !bookings.Any())
			{
				return NotFound();
			}

			return Ok(bookings);
		}

		[HttpPut("{bookingId:int}/cancel")]
		public async Task<ActionResult<BookingDto>> CancelBookingAsync(int bookingId)
		{
			var result = await _bookingService.CancelBookingByIdAsync(bookingId);
			return Ok(result);
		}
	}
}