using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Services.Interface;

namespace RentalManagementPlatformWebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BookingController : ControllerBase
	{
		private readonly IBookingService _bookingService;

		public BookingController(IBookingService bookingService, RentalManagementPlatformSqlContext context)
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

		[HttpPost]
		public async Task<ActionResult<BookingDto>> CreateBookingAsync([FromBody] CreateBookingDto dto)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var createdBooking = await _bookingService.CreateBookingAsync(dto);

				return CreatedAtAction(
					nameof(GetBookingById),
					new { bookingId = createdBooking.BookingId },
					createdBooking
				);
			}
			catch (Exception ex)
			{
				// ⭐ 詳細錯誤訊息
				return StatusCode(500, new
				{
					message = ex.Message,
					innerException = ex.InnerException?.Message,
					stackTrace = ex.StackTrace  // 開發環境才用
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