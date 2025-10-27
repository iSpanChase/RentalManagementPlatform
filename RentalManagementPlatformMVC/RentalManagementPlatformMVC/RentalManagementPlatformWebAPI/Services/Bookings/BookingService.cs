using AutoMapper;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.DTOs.Bookings;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;
using RentalManagementPlatformWebAPI.Services.Interfaces;

namespace RentalManagementPlatformWebAPI.Services.Bookings
{
	public class BookingService : IBookingService
	{
		private readonly IBookingRepository _bookingRepository;
		private readonly IRoomRepository _roomRepository;
		private readonly ICouponRepository _couponRepository;
		private readonly IMapper _mapper;
		private readonly ECPayService _ecpayService;

		public BookingService(IBookingRepository bookingRepository, IRoomRepository roomRepository, ICouponRepository couponRepository, IMapper mapper, ECPayService ecpayService)
		{
			_bookingRepository = bookingRepository;
			_roomRepository = roomRepository;
			_couponRepository = couponRepository;
			_mapper = mapper;
			_ecpayService = ecpayService;
		}

		// 取得所有訂單(測試用)
		public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync()
		{
			var bookings = await _bookingRepository.GetAllBookingsAsync();
			return _mapper.Map<IEnumerable<BookingDto>>(bookings);
		}

		/// <summary>
		/// 建立訂單並產生綠界付款表單
		/// </summary>
		public async Task<CreateOrderAndPayResponseDto> CreateBookingWithPaymentAsync(CreateBookingWithPaymentDto dto)
		{
			// 1. 驗證資料
			if (dto.CheckIn >= dto.CheckOut)
			{
				throw new ArgumentException("退房日期必須晚於入住日期");
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

			// 2. 生成訂單編號
			string orderNumber = await GenerateBookingNumberAsync();

			// 3. 建立訂單
			var booking = new Booking
			{
				// 基本資料
				GuestId = dto.GuestId,
				RoomId = dto.RoomId,
				CouponId = dto.CouponId,
				CheckIn = dto.CheckIn,
				CheckOut = dto.CheckOut,
				TotalPrice = dto.TotalPrice,
				OrderNumber = orderNumber,
				Status = "Pending",  // 等待付款
				CreatedAt = DateTime.Now,
				CommissionRateSnapshot = 0.15m,

				// 住宿資訊
				GuestCount = dto.GuestCount,
				PaymentTiming = dto.PaymentTiming,

				// 聯絡人資訊
				ContactName = dto.BillingInfo.Name,
				ContactEmail = dto.BillingInfo.Email,
				ContactPhone = dto.BillingInfo.Phone,
				ContactNotes = dto.BillingInfo.Notes,

				// 帳單地址
				BillingCountry = dto.BillingAddress.Country,
				BillingStreet = dto.BillingAddress.Street,
				BillingApartment = dto.BillingAddress.Apartment,
				BillingCity = dto.BillingAddress.City,
				BillingState = dto.BillingAddress.State,
				BillingZipCode = dto.BillingAddress.ZipCode,

				// 點數
				PointsRedeemed = dto.PointsRedeemed,
				PointsEarned = (int)Math.Floor(dto.TotalPrice * 0.01m)  // 1% 回饋
			};

			// 4. 儲存訂單到資料庫
			await _bookingRepository.CreateBookingAsync(booking);

			// 5. 產生綠界付款表單
			string itemName = $"{room.Title} ({dto.Nights}晚)";
			string ecpayFormHtml = _ecpayService.GeneratePaymentForm(
				orderNumber,
				dto.TotalPrice,
				itemName
			);

			// 6. 回傳結果
			return new CreateOrderAndPayResponseDto
			{
				BookingId = booking.BookingId,
				OrderNumber = orderNumber,
				EcpayFormHtml = ecpayFormHtml
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
