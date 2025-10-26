using AutoMapper;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Interface;
using RentalManagementPlatformWebAPI.Services.Interface;

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
			if (dto.CheckIn >= dto.CheckOut)
			{
				throw new ArgumentException("退房日期必須晚於住宿日期");
			}

			var room = await _roomRepository.GetByIdAsync(dto.RoomId)
				?? throw new ArgumentException("找不到指定房間");

			int nights = (dto.CheckOut - dto.CheckIn).Days;
			if (nights <= 0)
			{
				throw new ArgumentException("住宿天數必須大於 0");
			}

			// 計算總金額
			decimal totalPrice = room.PricePerNight.HasValue
				? room.PricePerNight.Value * nights
				: throw new ArgumentException("房間價格不可為空值");

			decimal discountAmount = 0m;

			// 優惠券折扣
			if (dto.CouponId.HasValue)
			{
				var coupon = await _couponRepository.GetByIdAsync(dto.CouponId.Value);
				if (coupon != null && totalPrice >= coupon.LowSpend)
				{
					if (coupon.DiscountMethod == "Amount")
					{
						discountAmount = coupon.DiscountQuota ?? 0m;
					}
					else if (coupon.DiscountMethod == "Percent")
					{
						discountAmount = totalPrice * ((coupon.DiscountQuota ?? 0m) / 100m);
					}

					totalPrice -= discountAmount;

					if (totalPrice < 0)
						totalPrice = 0;
				}
			}

			// 點數折抵
			if (dto.PointsRedeemed.HasValue && dto.PointsRedeemed > 0)
			{
				// 假設 1 點 = 1 元
				totalPrice -= (decimal)dto.PointsRedeemed.Value;

				if (totalPrice < 0)
				{
					totalPrice = 0;
				}
			}

			// 回饋點數
			int pointsEarned = (int)Math.Floor(totalPrice * 0.01m);

			// 生成訂單編號
			string orderNumber = await GenerateBookingNumberAsync();

			// 建立訂單物件 
			var booking = new Booking
			{
				GuestId = dto.GuestId,
				RoomId = dto.RoomId,
				CouponId = dto.CouponId,
				CheckIn = dto.CheckIn,
				CheckOut = dto.CheckOut,
				PointsRedeemed = dto.PointsRedeemed,
				PointsEarned = pointsEarned,
				TotalPrice = totalPrice,
				OrderNumber = orderNumber,
				Status = "Pending",
				CreatedAt = DateTime.Now,
				CommissionRateSnapshot = 0.15m,
			};

			await _bookingRepository.CreateBookingAsync(booking);

			// 回傳 DTO
			return new BookingDto
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

		public async Task<BookingDto?> CancelBookingByIdAsync(int bookingId)
		{
			var booking = await _bookingRepository.GetBookingByIdSimpleAsync(bookingId);
			if (booking == null)
			{
				throw new ArgumentException("此筆訂單不存在");
			}

			if (booking.Status == "Cancelled")
			{
				throw new InvalidOperationException("此預訂已經被取消");
			}

			booking.Status = "Cancelled";
			//booking.CancelledAt = DateTime.Now; 

			await _bookingRepository.CancelBookingByIdAsync(booking);

			return _mapper.Map<BookingDto>(booking);
		}


		// 內部使用方法
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
