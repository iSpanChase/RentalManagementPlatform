using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Repositories;
using AutoMapper;
using RentalManagementPlatformWebAPI.Repositories.Interface;

namespace RentalManagementPlatformWebAPI.Services
{
	public class BookingService : IBookingService
	{
		private readonly IBookingRepository _bookingRepository;
		private readonly IRoomRepository _roomRepository;
		private readonly ICouponRepository _couponRepository;
		private readonly IMapper _mapper;

		public BookingService(IBookingRepository bookingRepository, IRoomRepository roomRepository, ICouponRepository couponRepository, IMapper mapper)
		{
			_bookingRepository = bookingRepository;
			_roomRepository = roomRepository;
			_couponRepository = couponRepository;
			_mapper = mapper;
		}

		// 取得所有訂單(測試用)
		public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync()
		{
			var bookings = await _bookingRepository.GetAllBookingsAsync();
			return _mapper.Map<IEnumerable<BookingDto>>(bookings);
		}

		// 建立訂單，計算總價並應用優惠券等邏輯
		public async Task<BookingDto> CreateBookingAsync(CreateBookingDto dto)
		{
			// 基本驗證
			if (dto.CheckIn >= dto.CheckOut)
			{
				throw new ArgumentException("退房日期必須晚於住宿日期");
			}

			// 取得房間資訊
			var room = await _roomRepository.GetByIdAsync(dto.RoomId);
			if (room == null)
			{
				throw new ArgumentException("找不到指定房間");
			}

			// 計算住宿天數
			int nights = (dto.CheckOut - dto.CheckIn).Days;
			if (nights <= 0)
			{
				throw new ArgumentException("住宿天數必須大於 0");
			}

			// 計算總價
			decimal totalPrice = room.PricePerNight.HasValue
				? room.PricePerNight.Value * nights
				: throw new ArgumentException("房間價格不可為 Null");

			// 應用優惠券
			//if (dto.CouponId.HasValue)
			//{
			//	var coupon = await _couponRepository.GetByIdAsync(dto.CouponId.Value);
			//	if (coupon != null)
			//	{
			//		totalPrice -= coupon.DiscountAmount;
			//	}
			//}

			// 生成訂單編號
			string orderNumber = await GenerateBookingNumberAsync();

			var booking = new Booking
			{
				GuestId = dto.GuestId,
				RoomId = dto.RoomId,
				CouponId = dto.CouponId,
				CheckIn = dto.CheckIn,
				CheckOut = dto.CheckOut,
				PointsRedeemed = dto.PointsRedeemed,
				TotalPrice = totalPrice,
				OrderNumber = orderNumber,
				Status = "Pending",
				CreatedAt = DateTime.Now,
				CommissionRateSnapshot = 0.15m,
			};

			await _bookingRepository.CreateBookingAsync(booking);

			var bookingDto = new BookingDto
			{
				BookingId = booking.BookingId,
				GuestId = booking.GuestId,
				RoomId = booking.RoomId,
				CouponId = booking.CouponId,
				CheckIn = booking.CheckIn,
				CheckOut = booking.CheckOut,
				PointsRedeemed = booking.PointsRedeemed,
				TotalPrice = booking.TotalPrice,
				OrderNumber = booking.OrderNumber,
				Status = booking.Status,
				CreatedAt = booking.CreatedAt,
				Room = room.Title,
			};

			return bookingDto;
		}

		public async Task<BookingDto?> GetBookingByIdAsync(int bookingId)
		{
			var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
			return _mapper.Map<BookingDto?>(booking);
		}

		public async Task<IEnumerable<BookingDto>> GetBookingsByUserAsync(int userId)
		{
			var booking = await _bookingRepository.GetBookingsByUserAsync(userId);
			return _mapper.Map<IEnumerable<BookingDto>>(booking);
		}

		// 生成訂單編號，格式：ORD + yyyyMMdd + 4位流水號
		private async Task<string> GenerateBookingNumberAsync()
		{
			string datePrefix = DateTime.Now.ToString("yyyyMMdd");
			
			// 透過 Repository 查詢當天最後一筆訂單
			string? lastBookingNumber = await _bookingRepository
				.GetLastBookingNumberByDateAsync(datePrefix);

			int sequence = 1;

			if (!string.IsNullOrEmpty(lastBookingNumber))
			{
				// 解析最後4位數字：ORD202402290003 → "0003"
				string lastSeq = lastBookingNumber.Substring(lastBookingNumber.Length - 4);
				sequence = int.Parse(lastSeq) + 1;
			}

			// 組合訂單編號：ORD + 日期 + 流水號(4位)
			return $"ORD{datePrefix}{sequence:D4}";
		}
	}
}
