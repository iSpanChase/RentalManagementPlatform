using AutoMapper;
using RentalManagementPlatformWebAPI.DTOs.Payments;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;
using RentalManagementPlatformWebAPI.Services.Interfaces;

namespace RentalManagementPlatformWebAPI.Services.Payments
{
	public class PaymentsService : IPaymentsService
	{
		private readonly IBookingRepository _bookingRepository;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly IMapper _mapper;
		private readonly ECPayService _ecpayService;
		private readonly ILogger<PaymentsService> _logger;

		public PaymentsService(IPaymentsRepository paymentsRepository, IBookingRepository bookingRepository, IMapper mapper, ECPayService ecpayService, ILogger<PaymentsService> logger)
		{
			_paymentsRepository = paymentsRepository;
			_bookingRepository = bookingRepository;
			_mapper = mapper;
			_ecpayService = ecpayService;
			_logger = logger;
		}

		public async Task<IEnumerable<PaymentsDto>> GetAllPaymentsAsync()
		{
			var payments = await _paymentsRepository.GetAllPaymentsAsync();
			return _mapper.Map<IEnumerable<PaymentsDto>>(payments);
		}

		// 處理綠界付款回調
		public async Task<PaymentCallbackResultDto> ProcessEcpayCallbackAsync(
			string orderNumber,
			bool isSuccess,
			string tradeNo,
			decimal tradeAmount,
			DateTime paymentDate,
			string paymentType)
		{
			_logger.LogInformation("=== PaymentService: 處理綠界付款回調 ===");
			_logger.LogInformation($"訂單編號：{orderNumber}");
			_logger.LogInformation($"付款結果：{(isSuccess ? "成功" : "失敗")}");
			_logger.LogInformation($"交易編號：{tradeNo}");
			_logger.LogInformation($"交易金額：{tradeAmount}");
			_logger.LogInformation($"付款方式：{paymentType}");

			try
			{
				// ==================== 1. 查詢訂單 ====================
				_logger.LogInformation("步驟 1：查詢訂單...");

				var booking = await _bookingRepository.GetBookingByOrderNumberAsync(orderNumber);

				if (booking == null)
				{
					_logger.LogError($"找不到訂單：{orderNumber}");
					return new PaymentCallbackResultDto
					{
						Success = false,
						Message = "找不到訂單",
						OrderNumber = orderNumber,
						PaymentStatus = "",
						OrderStatus = ""
					};
				}

				_logger.LogInformation($"找到訂單");
				_logger.LogInformation($"BookingId：{booking.BookingId}");
				_logger.LogInformation($"目前付款狀態：{booking.PaymentStatus}");
				_logger.LogInformation($"目前訂單狀態：{booking.Status}");

				// ==================== 2. 檢查是否已處理過 ====================
				if (booking.PaymentStatus == "completed")
				{
					_logger.LogWarning("此訂單已完成付款，可能是重複通知");
					return new PaymentCallbackResultDto
					{
						Success = true,
						Message = "訂單已完成付款（重複通知）",
						OrderNumber = orderNumber,
						PaymentStatus = booking.PaymentStatus,
						OrderStatus = booking.Status
					};
				}

				// ==================== 3. 驗證付款金額 ====================
				_logger.LogInformation("步驟 2：驗證付款金額...");

				if (booking.TotalPrice.HasValue)
				{
					decimal bookingAmount = booking.TotalPrice.Value;

					if (Math.Abs(bookingAmount - tradeAmount) > 0.01m)  // 允許 0.01 的誤差
					{
						_logger.LogWarning($"付款金額不符！訂單金額：{bookingAmount}，付款金額：{tradeAmount}");

						// 金額不符，但還是更新狀態（標記為需要人工審核）
						booking.PaymentStatus = "pending_review";
						await _bookingRepository.UpdateBookingAsync(booking);

						return new PaymentCallbackResultDto
						{
							Success = false,
							Message = "付款金額與訂單金額不符，需要人工審核",
							OrderNumber = orderNumber,
							PaymentStatus = booking.PaymentStatus,
							OrderStatus = booking.Status
						};
					}

					_logger.LogInformation($"金額驗證通過：{tradeAmount}");
				}
				else
				{
					_logger.LogWarning("訂單沒有設定金額");
				}

				// ==================== 4. 更新訂單狀態 ====================
				_logger.LogInformation("步驟 3：更新訂單狀態...");

				string oldPaymentStatus = booking.PaymentStatus;
				string oldOrderStatus = booking.Status;

				if (isSuccess)
				{
					// ========== 付款成功 ==========
					_logger.LogInformation("付款成功");

					booking.PaymentStatus = "completed";  // 付款狀態：已完成
					booking.Status = "Confirmed";  // 訂單狀態：已確認

					_logger.LogInformation($"   PaymentStatus: {oldPaymentStatus} → completed");
					_logger.LogInformation($"   Status: {oldOrderStatus} → Confirmed");
				}
				else
				{
					// ========== 付款失敗 ==========
					_logger.LogWarning("付款失敗");

					booking.PaymentStatus = "failed";  // 付款狀態：失敗
													   // Status 保持Pending

					_logger.LogInformation($"PaymentStatus: {oldPaymentStatus} → failed");
					_logger.LogInformation($"Status: {oldOrderStatus} (保持不變)");
				}

				// ==================== 5. 儲存到資料庫 ====================
				await _bookingRepository.UpdateBookingAsync(booking);

				_logger.LogInformation("訂單狀態更新成功");

				// ==================== 6. 記錄付款資訊 ====================
				/*
				var payment = new Payment
				{
					BookingId = booking.BookingId,
					TradeNo = tradeNo,
					Amount = tradeAmount,
					PaymentDate = paymentDate,
					PaymentType = paymentType,
					Status = isSuccess ? "Success" : "Failed",
					CreatedAt = DateTime.Now
				};
				await _paymentRepository.CreateAsync(payment);
				_logger.LogInformation("付款記錄已建立");
				*/

				// ==================== 7. 回傳結果 ====================
				return new PaymentCallbackResultDto
				{
					Success = true,
					Message = isSuccess ? "付款成功" : "付款失敗",
					OrderNumber = orderNumber,
					PaymentStatus = booking.PaymentStatus,
					OrderStatus = booking.Status
				};
			}
			catch (Exception ex)
			{
				_logger.LogError($"處理綠界付款回調時發生錯誤：{ex.Message}");
				_logger.LogError($"錯誤堆疊：{ex.StackTrace}");

				return new PaymentCallbackResultDto
				{
					Success = false,
					Message = $"系統錯誤：{ex.Message}",
					OrderNumber = orderNumber,
					PaymentStatus = "",
					OrderStatus = ""
				};
			}
		}

		/// <summary>
		/// 為延後支付的訂單產生綠界付款表單
		/// </summary>
		public async Task<string> GeneratePaymentFormForDeferredBookingAsync(string orderNumber)
		{
			_logger.LogInformation($"=== PaymentService: 為延後支付訂單產生付款表單 ===");
			_logger.LogInformation($"訂單編號：{orderNumber}");

			try
			{
				// 1. 查詢訂單
				var booking = await _bookingRepository.GetBookingByOrderNumberAsync(orderNumber);

				if (booking == null)
				{
					throw new ArgumentException($"找不到訂單：{orderNumber}");
				}

				// 2. 驗證訂單狀態
				if (booking.PaymentStatus == "completed")
				{
					throw new InvalidOperationException("此訂單已完成付款");
				}

				if (booking.PaymentTiming != "partial")
				{
					throw new InvalidOperationException("此訂單不是延後支付");
				}

				if (booking.PaymentDeadline.HasValue && DateTime.Now > booking.PaymentDeadline.Value)
				{
					throw new InvalidOperationException("付款期限已過");
				}

				// 3. 產生綠界表單
				if (!booking.TotalPrice.HasValue)
				{
					throw new InvalidOperationException("訂單金額錯誤");
				}

				// 取得房間資訊（用於顯示品項名稱）
				string itemName = $"訂單編號：{orderNumber}";

				// if (booking.Room != null)
				// {
				//     int nights = (booking.CheckOut.Value - booking.CheckIn.Value).Days;
				//     itemName = $"{booking.Room.Title} ({nights}晚)";
				// }

				string ecpayFormHtml = _ecpayService.GeneratePaymentForm(
					orderNumber,
					booking.TotalPrice.Value,
					itemName
				);

				_logger.LogInformation("綠界表單產生成功");

				return ecpayFormHtml;
			}
			catch (Exception ex)
			{
				_logger.LogError($"產生付款表單失敗：{ex.Message}");
				throw;
			}
		}
	}
}
