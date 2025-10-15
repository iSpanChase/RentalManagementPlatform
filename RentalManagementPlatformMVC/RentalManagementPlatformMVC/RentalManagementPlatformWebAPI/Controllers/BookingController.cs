using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services;

namespace RentalManagementPlatformWebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BookingController : ControllerBase
	{
		private readonly IBookingService _bookingService;

		public BookingController(IBookingService bookingService)
		{
			_bookingService = bookingService;
		}

		// 取得所有訂單(測試用)
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Booking>>> GetAllBookings()
		{
			var bookings = await _bookingService.GetAllBookingsAsync();
			return Ok(bookings);
		}

		[HttpPost]
		public async Task<ActionResult<BookingDto>> CreateBookingAsync([FromBody] CreateBookingDto dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var createdBooking = await _bookingService.CreateBookingAsync(dto);

			return CreatedAtAction(
				nameof(GetBookingById), // 對應的查詢方法
				new { bookingId = createdBooking.BookingId }, // 路由參數
				createdBooking // 回傳的內容
			);
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
		public async Task<ActionResult<IEnumerable<Booking>>> GetBookingByUser(int userId)
		{
			var bookings = await _bookingService.GetBookingsByUserAsync(userId);

			if (bookings == null || !bookings.Any())
			{
				return NotFound();
			}

			return Ok(bookings);
		}
	}
}