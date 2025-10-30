using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace RentalManagementPlatformWebAPI.Services.Payments
{
	public class ECPayService
	{
		private readonly IConfiguration _configuration;

		// 測試環境設定
		private readonly string _merchantId;
		private readonly string _hashKey;
		private readonly string _hashIV;
		private readonly string _ecpayUrl;
		private readonly string _returnUrl;
		private readonly string _orderResultUrl;

		public ECPayService(IConfiguration configuration)
		{
			_configuration = configuration;

			// 從 appsettings.json 讀取設定
			_merchantId = _configuration["ECPay:MerchantId"] ?? "2000132";
			_hashKey = _configuration["ECPay:HashKey"] ?? "5294y06JbISpM5x9";
			_hashIV = _configuration["ECPay:HashIV"] ?? "v77hoKGq4kWxNNIS";
			_ecpayUrl = _configuration["ECPay:PaymentUrl"] ?? "https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5";
			// ECPay URLs using paid ngrok static domains
			_returnUrl = "https://my-project-backend.ngrok.app/api/payments/return"; // Official: Server-side, for backend notification
			_orderResultUrl = "https://my-project-backend.ngrok.app/api/payments/redirect-handler"; // Official: Client-side, for user's browser (via backend redirect handler)
		}

		/// <summary>
		/// 產生綠界付款表單 HTML
		/// </summary>
		public string GeneratePaymentForm(string orderNumber, decimal totalAmount, string itemName)
		{
			// 綠界要求金額必須是整數
			int amount = (int)Math.Round(totalAmount);

			// 建立付款參數
			var parameters = new Dictionary<string, string>
			{
				{ "MerchantID", _merchantId },
				{ "MerchantTradeNo", orderNumber },
				{ "MerchantTradeDate", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") },
				{ "PaymentType", "aio" },
				{ "TotalAmount", amount.ToString() },
				{ "TradeDesc", "房間預訂" },
				{ "ItemName", itemName },
				{ "ReturnURL", _returnUrl },
				{ "OrderResultURL", _orderResultUrl },
				{ "ChoosePayment", "Credit" },  // 只開放信用卡
                { "EncryptType", "1" }  // SHA256
            };

			// 計算檢查碼
			string checkMacValue = GenerateCheckMacValue(parameters);
			parameters.Add("CheckMacValue", checkMacValue);

			// 產生 HTML Form
			return GenerateHtmlForm(parameters);
		}

		/// <summary>
		/// 計算檢查碼（CheckMacValue）
		/// </summary>
		private string GenerateCheckMacValue(Dictionary<string, string> parameters)
		{
			// 1. 參數依照字母排序
			var sortedParams = parameters.OrderBy(x => x.Key).ToList();

			// 2. 組合字串：HashKey + 參數 + HashIV
			var builder = new StringBuilder();
			builder.Append($"HashKey={_hashKey}");

			foreach (var param in sortedParams)
			{
				builder.Append($"&{param.Key}={param.Value}");
			}

			builder.Append($"&HashIV={_hashIV}");

			// 3. URL Encode
			string raw = builder.ToString();
			string encoded = HttpUtility.UrlEncode(raw).ToLower();

			// 4. SHA256 雜湊
			using (var sha256 = SHA256.Create())
			{
				byte[] bytes = Encoding.UTF8.GetBytes(encoded);
				byte[] hash = sha256.ComputeHash(bytes);

				// 5. 轉換為大寫
				return BitConverter.ToString(hash).Replace("-", "").ToUpper();
			}
		}

		/// <summary>
		/// 產生 HTML Form（會自動提交到綠界）
		/// </summary>
		private string GenerateHtmlForm(Dictionary<string, string> parameters)
		{
			var formBuilder = new StringBuilder();

			formBuilder.AppendLine("<form id=\"ecpay-form\" method=\"post\" action=\"" + _ecpayUrl + "\">");

			foreach (var param in parameters)
			{
				formBuilder.AppendLine($"  <input type=\"hidden\" name=\"{param.Key}\" value=\"{param.Value}\" />");
			}

			formBuilder.AppendLine("</form>");
			formBuilder.AppendLine("<script>");
			formBuilder.AppendLine("  document.getElementById('ecpay-form').submit();");
			formBuilder.AppendLine("</script>");

			return formBuilder.ToString();
		}

		/// <summary>
		/// 驗證綠界回傳的檢查碼
		/// </summary>
		public bool ValidateCheckMacValue(Dictionary<string, string> parameters)
		{
			if (!parameters.ContainsKey("CheckMacValue"))
				return false;

			string receivedCheckMacValue = parameters["CheckMacValue"];
			parameters.Remove("CheckMacValue");

			string calculatedCheckMacValue = GenerateCheckMacValue(parameters);

			return receivedCheckMacValue.Equals(calculatedCheckMacValue, StringComparison.OrdinalIgnoreCase);
		}
	}
}
