using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOs.Payments;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using RentalManagementPlatformWebAPI.Services.Payments;

namespace RentalManagementPlatformWebAPI.Area.Payments.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PaymentsController : ControllerBase
	{
		private readonly IPaymentsService _paymentsService;
		private readonly ILogger<PaymentsController> _logger;
		private readonly ECPayService _ecPayService;

		public PaymentsController(IPaymentsService paymentsService, ILogger<PaymentsController> logger, ECPayService ecpayService)
		{
			_paymentsService = paymentsService;
			_logger = logger;
			_ecPayService = ecpayService;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<PaymentsDto>>> GetAllPaymentsAsync()
		{
			var payments = await _paymentsService.GetAllPaymentsAsync();
			return Ok(payments);
		}

		/// <summary>
		/// 為延後支付的訂單產生付款表單
		/// </summary>
		[HttpGet("deferred/{orderNumber}")]
		public async Task<IActionResult> GenerateDeferredPaymentForm(string orderNumber)
		{
			try
			{
				_logger.LogInformation($"為延後支付訂單 {orderNumber} 產生付款表單");

				string ecpayFormHtml = await _paymentsService.GeneratePaymentFormForDeferredBookingAsync(orderNumber);

				return Ok(new
				{
					success = true,
					orderNumber = orderNumber,
					ecpayFormHtml = ecpayFormHtml
				});
			}
			catch (Exception ex)
			{
				_logger.LogError($"產生付款表單失敗 for order {orderNumber}: {ex.Message}");

				return BadRequest(new
				{
					success = false,
					message = ex.Message
				});
			}
		}

		/// <summary>
		/// 接收綠界付款結果通知（ReturnURL）
		/// </summary>
		[HttpPost("return")]
		public async Task<IActionResult> EcpayReturn([FromForm] Dictionary<string, string> formData)
		{
			try
			{
				_logger.LogInformation("=== 收到綠界回調 ===");
				_logger.LogInformation($"回調時間：{DateTime.Now}");

				// 記錄參數
				_logger.LogInformation("回調參數：");
				foreach (var param in formData)
				{
					if (param.Key != "CheckMacValue")
					{
						_logger.LogInformation($"  {param.Key}: {param.Value}");
					}
				}

				// ==================== 1. 驗證檢查碼 ====================
				_logger.LogInformation("\n步驟 1：驗證 CheckMacValue...");

				bool isValid = _ecPayService.ValidateCheckMacValue(formData);

				if (!isValid)
				{
					_logger.LogError("CheckMacValue 驗證失敗！");
					return Content("0|CheckMacValue驗證失敗");
				}

				_logger.LogInformation("CheckMacValue 驗證通過");

				// ==================== 2. 取得付款資訊 ====================
				_logger.LogInformation("\n步驟 2：解析付款資訊...");

				string merchantTradeNo = formData["MerchantTradeNo"];  // 訂單編號
				string rtnCode = formData["RtnCode"];  // 付款結果代碼
				string rtnMsg = formData["RtnMsg"];  // 付款結果訊息
				string tradeNo = formData["TradeNo"];  // 綠界交易編號
				string tradeAmt = formData["TradeAmt"];  // 交易金額
				string paymentDate = formData["PaymentDate"];  // 付款時間
				string paymentType = formData.ContainsKey("PaymentType") ? formData["PaymentType"] : "";

				_logger.LogInformation($"  訂單編號：{merchantTradeNo}");
				_logger.LogInformation($"  付款結果：{rtnCode} - {rtnMsg}");
				_logger.LogInformation($"  綠界交易號：{tradeNo}");
				_logger.LogInformation($"  交易金額：{tradeAmt}");
				_logger.LogInformation($"  付款時間：{paymentDate}");
				_logger.LogInformation($"  付款方式：{paymentType}");

				// ==================== 3. 呼叫 PaymentService 處理 ====================
				_logger.LogInformation("\n步驟 3：呼叫 PaymentService 處理付款回調...");

				bool isSuccess = rtnCode == "1";  // 1 = 付款成功
				decimal amount = decimal.Parse(tradeAmt);
				DateTime paymentDateTime = DateTime.Parse(paymentDate);

				var result = await _paymentsService.ProcessEcpayCallbackAsync(
					orderNumber: merchantTradeNo,
					isSuccess: isSuccess,
					tradeNo: tradeNo,
					tradeAmount: amount,
					paymentDate: paymentDateTime,
					paymentType: paymentType
				);

				// ==================== 4. 回傳結果給綠界 ====================
				if (result.Success)
				{
					_logger.LogInformation("\n=== 付款回調處理完成 ===");
					_logger.LogInformation($"訂單編號：{result.OrderNumber}");
					_logger.LogInformation($"付款狀態：{result.PaymentStatus}");
					_logger.LogInformation($"訂單狀態：{result.OrderStatus}");
					_logger.LogInformation($"訊息：{result.Message}");

					// 回傳 "1|OK" 給綠界（表示我們已經處理完成）
					return Content("1|OK");
				}
				else
				{
					_logger.LogError($"\n=== 付款回調處理失敗 ===");
					_logger.LogError($"錯誤訊息：{result.Message}");

					return Content($"0|{result.Message}");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"\n=== 處理綠界回調時發生例外 ===");
				_logger.LogError($"錯誤訊息：{ex.Message}");
				_logger.LogError($"錯誤堆疊：{ex.StackTrace}");

				return Content("0|系統錯誤");
			}
		}

		/// <summary>
		/// 接收綠界付款結果通知，並導向前端頁面 (OrderResultURL)
		/// </summary>
		[HttpPost("redirect-handler")]
		public IActionResult EcpayFrontendRedirect([FromForm] Dictionary<string, string> formData)
		{
		    _logger.LogInformation("收到綠界 OrderResultURL 請求，準備導向前端訂單頁面。");

		    // 從 formData 中取得 MerchantTradeNo (即訂單編號)
		    string? orderNumber = formData.ContainsKey("MerchantTradeNo") ? formData["MerchantTradeNo"] : null;

		    if (!string.IsNullOrEmpty(orderNumber))
		    {
		        // 導向到前端的訂單頁面，並帶上訂單編號
		        return Redirect($"https://my-project-frontend.ngrok.app/booking/mybookings?orderNumber={orderNumber}");
		    }
		    else
		    {
		        _logger.LogWarning("從綠界 OrderResultURL 回調中未取得 MerchantTradeNo，導向通用訂單頁面。");
		        // 如果沒有訂單編號，則導向通用訂單頁面
		        return Redirect($"https://my-project-frontend.ngrok.app/booking/mybookings");
		    }
		}
		
		/// 測試用：手動觸發回調（開發時使用）
		/// 正式環境請移除此方法或加上權限驗證
		/// </summary>
		[HttpGet("test-callback")]
		public async Task<IActionResult> TestCallback(string orderNumber, bool success = true)
		{
			_logger.LogInformation($"測試付款回調");
			_logger.LogInformation($"訂單編號：{orderNumber}");
			_logger.LogInformation($"模擬結果：{(success ? "成功" : "失敗")}");

			try
			{
				// 模擬付款
				var result = await _paymentsService.ProcessEcpayCallbackAsync(
					orderNumber: orderNumber,
					isSuccess: success,
					tradeNo: "TEST" + DateTime.Now.ToString("yyyyMMddHHmmss"),
					tradeAmount: 6996,
					paymentDate: DateTime.Now,
					paymentType: "Credit_CreditCard"
				);

				return Ok(new
				{
					message = "測試回調完成",
					orderNumber = orderNumber,
					success = result.Success,
					paymentStatus = result.PaymentStatus,
					orderStatus = result.OrderStatus,
					resultMessage = result.Message
				});
			}
			catch (Exception ex)
			{
				_logger.LogError($"測試回調失敗：{ex.Message}");

				return BadRequest(new
				{
					message = "測試失敗",
					error = ex.Message
				});
			}
		}
	}
}
