using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOs.Bookings;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using System.Security.Claims;

namespace RentalManagementPlatformWebAPI.Area.Bookings.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize] // 整個控制器都需要認證
	public class BookingsController : ControllerBase
	{
		private readonly IBookingService _bookingService;

		public BookingsController(IBookingService bookingService, RentalManagementPlatformSqlContext context)
		{
			_bookingService = bookingService;
		}

		// [開發用] 根據 HostId 獲取其所有訂單
		[Obsolete("此方法已過時，請使用 GetMyBookingsAsync 方法")]
		[HttpGet("user/{guestId}")]
		public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookingsByUserId(int guestId)
		{
			// 驗證使用者只能查看自己的預訂
			var currentUserId = GetCurrentUserId();
			if (currentUserId != guestId)
			{
				return Forbid("您只能查看自己的預訂記錄");
			}

			var bookings = await _bookingService.GetBookingsByUserAsync(guestId);
			return Ok(bookings);
		}

		// [開發用] 根據 HostId 獲取其所有訂單
		[Obsolete("此方法已過時，請使用 GetMyOrdersAsync 方法")]
		[HttpGet("host/{hostId}")]
		public async Task<ActionResult<IEnumerable<BookingDto>>> GetOrdersByHostId(int hostId)
		{
			// 驗證使用者只能查看自己的訂單
			var currentUserId = GetCurrentUserId();
			if (currentUserId != hostId)
			{
				return Forbid("您只能查看自己的訂單記錄");
			}

			var bookings = await _bookingService.GetOrdersByHostIdAsync(hostId);
			return Ok(bookings);
		}

		// 根據已驗證 GuestId 獲取其所有預訂
		[HttpGet("my-bookings")]
		public async Task<ActionResult<IEnumerable<BookingDto>>> GetMyBookings()
		{
			var currentUserId = GetCurrentUserId();
			if (currentUserId == null)
			{
				return Unauthorized("無法識別使用者身份");
			}

			var bookings = await _bookingService.GetMyBookingsAsync(currentUserId.Value);
			return Ok(bookings);
		}

		// 根據已驗證 HostId 獲取其所有訂單
		[HttpGet("my-orders")]
		public async Task<ActionResult<IEnumerable<BookingDto>>> GetMyOrders()
		{
			var currentUserId = GetCurrentUserId();
			if (currentUserId == null)
			{
				return Unauthorized("無法識別使用者身份");
			}

			var bookings = await _bookingService.GetMyOrdersAsync(currentUserId.Value);
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

			// 驗證使用者是否有權限查看此訂單（房客或房東）
			var currentUserId = GetCurrentUserId();
			if (currentUserId == null)
			{
				return Unauthorized("無法識別使用者身份");
			}

			if (booking.GuestId != currentUserId && booking.HostId != currentUserId)
			{
				return Forbid("您沒有權限查看此訂單");
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

				// 驗證使用者身份與GuestId一致
				var currentUserId = GetCurrentUserId();
				if (currentUserId == null)
				{
					return Unauthorized("無法識別使用者身份");
				}

				if (dto.GuestId != currentUserId)
				{
					return Forbid("您只能為自己建立訂單");
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
			// 首先取得訂單資料以驗證權限
			var booking = await _bookingService.GetBookingByIdAsync(bookingId);
			if (booking == null)
			{
				return NotFound("找不到指定的訂單");
			}

			// 驗證使用者是否有權限取消此訂單（只有房客可以取消）
			var currentUserId = GetCurrentUserId();
			if (currentUserId == null)
			{
				return Unauthorized("無法識別使用者身份");
			}

			if (booking.GuestId != currentUserId)
			{
				return Forbid("您只能取消自己的預訂");
			}

			var result = await _bookingService.CancelBookingByIdAsync(bookingId);

			var apiResponse = new
			{
				success = true,
				message = result != null ? "訂單取消成功" : "找不到訂單或無法取消",
				booking = result
			};

			return Ok(apiResponse);
		}

		/// <summary>
		/// 從JWT Token中取得目前使用者的ID
		/// </summary>
		/// <returns>使用者ID，如果無法取得則返回null</returns>
		private int? GetCurrentUserId()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (int.TryParse(userIdClaim, out var userId))
			{
				return userId;
			}
			return null;
		}
	}
}