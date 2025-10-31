using AutoMapper;
using RentalManagementPlatformWebAPI.DTOs.Bookings;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using RentalManagementPlatformWebAPI.Services.Payments;
using Microsoft.AspNetCore.SignalR;
using RentalManagementPlatformWebAPI.Hubs;

namespace RentalManagementPlatformWebAPI.Services.Bookings
{
	public class BookingService : IBookingService
	{
		private readonly IBookingRepository _bookingRepository;
		private readonly IRoomRepository _roomRepository;
		private readonly ICouponRepository _couponRepository;
		private readonly IMapper _mapper;
		private readonly ECPayService _ecpayService;
        private readonly IHubContext<NotificationHub> _hubContext;
		private readonly IFileUrlResolver _fileUrlResolver; // 注入 IFileUrlResolver

		public BookingService(
				IBookingRepository bookingRepository, 
				IRoomRepository roomRepository, 
				ICouponRepository couponRepository, 
				IMapper mapper, 
				ECPayService ecpayService, 
				IHubContext<NotificationHub> hubContext,
				IFileUrlResolver fileUrlResolver
			) 
		{
			_bookingRepository = bookingRepository;
			_roomRepository = roomRepository;
			_couponRepository = couponRepository;
			_mapper = mapper;
			_ecpayService = ecpayService;
            _hubContext = hubContext;
			_fileUrlResolver = fileUrlResolver; 
		}

		// 根據 GuestId 獲取其所有訂單
		public async Task<IEnumerable<BookingDto>> GetBookingsByUserAsync(int guestId)
		{
			var bookings = await _bookingRepository.GetBookingsByGuestIdAsync(guestId);

			if (bookings == null || !bookings.Any())
			{
				throw new ArgumentException("找不到該使用者的訂單");
			}

			var bookingDtos = new List<BookingDto>();
			foreach (var booking in bookings)
			{
				var bookingDto = _mapper.Map<BookingDto>(booking);
				if (booking.Room != null && booking.Room.RoomPhotos != null && booking.Room.RoomPhotos.Any())
				{
					var mainPhoto = booking.Room.RoomPhotos.OrderBy(p => p.SortOrder).FirstOrDefault(p => p.PhotoType == "Cover") ?? booking.Room.RoomPhotos.OrderBy(p => p.SortOrder).FirstOrDefault();
					if (mainPhoto != null)
					{
						bookingDto.RoomImageUrl = await _fileUrlResolver.GetPhotoUrlAsync(mainPhoto);
					}
				}
				bookingDtos.Add(bookingDto);
			}

			return bookingDtos;
		}

		// 根據 HostId 獲取其所有訂單
		public async Task<IEnumerable<BookingDto>> GetOrdersByHostIdAsync(int hostId)
		{
			var bookings = await _bookingRepository.GetOrdersByHostIdAsync(hostId);
			if (bookings == null || !bookings.Any())
			{
				throw new ArgumentException("找不到該房東的訂單");
			}

			var bookingDtos = new List<BookingDto>();
			foreach (var booking in bookings)
			{
				var bookingDto = _mapper.Map<BookingDto>(booking);
				if (booking.Room != null && booking.Room.RoomPhotos != null && booking.Room.RoomPhotos.Any())
				{
					var mainPhoto = booking.Room.RoomPhotos.OrderBy(p => p.SortOrder).FirstOrDefault(p => p.PhotoType == "Cover") ?? booking.Room.RoomPhotos.OrderBy(p => p.SortOrder).FirstOrDefault();
					if (mainPhoto != null)
					{
						bookingDto.RoomImageUrl = await _fileUrlResolver.GetPhotoUrlAsync(mainPhoto);
					}
				}
				bookingDtos.Add(bookingDto);
			}

			return bookingDtos;
		}

		// 建立訂單並根據付款時機決定是否產生綠界表單
		public async Task<CreateOrderAndPayResponseDto> CreateBookingWithPaymentAsync(CreateBookingWithPaymentDto dto)
		{
			// ==================== 1. 驗證資料 ====================
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

			// ==================== 2. 計算總金額 ====================
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

			// ==================== 3. 生成訂單編號 ====================
			string orderNumber = await GenerateBookingNumberAsync();

			// ==================== 4. 建立訂單 ====================
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
				Status = "Pending",  // 訂單狀態：等待確認
				CreatedAt = DateTime.Now,
				CommissionRateSnapshot = 0.15m,

				// 住宿資訊
				GuestCount = dto.GuestCount,
				PaymentTiming = dto.PaymentTiming,  // "full" 或 "partial"

				// 關鍵：根據付款時機設定付款狀態
				PaymentStatus = dto.PaymentTiming == "full" ? "pending" : "deferred",

				// 設定付款期限（入住前一天）
				PaymentDeadline = dto.CheckIn.AddDays(-7).Date.AddHours(23).AddMinutes(59).AddSeconds(59),

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

			// ==================== 5. 儲存訂單到資料庫 ====================
			await _bookingRepository.CreateBookingAsync(booking);

			Console.WriteLine($"訂單建立成功：{orderNumber}");
			Console.WriteLine($"付款時機：{dto.PaymentTiming}");
			Console.WriteLine($"付款狀態：{booking.PaymentStatus}");

            // ==================== 5-1. 發送新訂單即時通知給房東 ====================
            if (room != null && room.HostId.HasValue)
            {
                var hostId = room.HostId.Value;
                string message = $"新訂單通知：您的房源 '{room.Title}' 有一筆新訂單 (訂單編號: {booking.OrderNumber})，入住日：{booking.CheckIn:yyyy-MM-dd}。";
                await _hubContext.Clients.Group($"host_{hostId}").SendAsync("ReceiveWarning", message);//傳message給"ReceiveWarning"監聽器
            }

            // ==================== 6. 根據付款時機決定是否產生綠界表單 ====================

            if (dto.PaymentTiming == "full")
			{
				// ========== 立即支付：產生綠界表單 ==========
				Console.WriteLine("立即支付：產生綠界表單...");

				string itemName = $"{room.Title} ({nights}晚)";
				string ecpayFormHtml = _ecpayService.GeneratePaymentForm(
					orderNumber,
					dto.TotalPrice,
					itemName
				);

				Console.WriteLine($"綠界表單產生成功，長度：{ecpayFormHtml.Length}");

				return new CreateOrderAndPayResponseDto
				{
					BookingId = booking.BookingId,
					OrderNumber = orderNumber,

					// 立即支付的回傳資料
					PaymentRequired = true,
					PaymentStatus = "pending",
					PaymentDeadline = null,
					EcpayFormHtml = ecpayFormHtml
				};
			}
			else if (dto.PaymentTiming == "partial")
			{
				// ========== 延後支付：不產生表單 ==========
				Console.WriteLine("延後支付：不產生綠界表單");
				Console.WriteLine($"付款期限：{booking.PaymentDeadline}");

				return new CreateOrderAndPayResponseDto
				{
					BookingId = booking.BookingId,
					OrderNumber = orderNumber,

					// 延後支付的回傳資料
					PaymentRequired = false,
					PaymentStatus = "deferred",
					PaymentDeadline = booking.PaymentDeadline,
					EcpayFormHtml = null
				};
			}
			else
			{
				throw new ArgumentException($"無效的付款時機：{dto.PaymentTiming}");
			}
		}

		// 根據 BookingId 取消訂單
		public async Task<BookingDto?> CancelBookingByIdAsync(int bookingId)
		{
			var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);

			if (booking == null)
			{
				throw new ArgumentException("找不到指定訂單");
			}

			if (booking.Status == "Cancelled")
			{
				throw new InvalidOperationException("訂單已經取消");
			}

			if (booking.Status == "Completed")
			{
				throw new InvalidOperationException("已完成的訂單無法取消");
			}

			booking.Status = "Cancelled";

			if (booking.PaymentStatus == "completed")
			{
				booking.PaymentStatus = "refunded";
			}
			else
			{
				booking.PaymentStatus = "cancelled";
			}

			booking.UpdatedAt = DateTime.Now;
			await _bookingRepository.UpdateBookingAsync(booking);
			return _mapper.Map<BookingDto>(booking);
		}

		// 根據訂單編號獲取單一訂單詳情
		public async Task<BookingDto?> GetBookingByOrderNumberAsync(string orderNumber)
		{
			var booking = await _bookingRepository.GetBookingByOrderNumberAsync(orderNumber);
			if (booking == null)
			{
				return null;
			}

			var bookingDto = _mapper.Map<BookingDto>(booking);
			if (booking.Room != null && booking.Room.RoomPhotos != null && booking.Room.RoomPhotos.Any())
			{
				var mainPhoto = booking.Room.RoomPhotos.OrderBy(p => p.SortOrder).FirstOrDefault(p => p.PhotoType == "Cover") ?? booking.Room.RoomPhotos.OrderBy(p => p.SortOrder).FirstOrDefault();
				if (mainPhoto != null)
				{
					bookingDto.RoomImageUrl = await _fileUrlResolver.GetPhotoUrlAsync(mainPhoto);
				}
			}
			return bookingDto;
		}

		// ==================== 內部使用方法 ====================

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
			return $"ORD{datePrefix}{sequence:D4}{new Random().Next(1000, 9999)}";
		}
	}
}