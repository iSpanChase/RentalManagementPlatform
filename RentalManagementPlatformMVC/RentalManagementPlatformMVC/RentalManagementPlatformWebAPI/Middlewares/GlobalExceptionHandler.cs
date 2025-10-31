using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace RentalManagementPlatformWebAPI.Middlewares
{
	public class GlobalExceptionHandler : IExceptionHandler
	{
		private readonly ILogger<GlobalExceptionHandler> _logger;

		public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
		{
			_logger = logger;
		}

		public async ValueTask<bool> TryHandleAsync(
			HttpContext httpContext,
			Exception exception,
			CancellationToken cancellationToken)
		{
			// 記錄詳細錯誤資訊
			_logger.LogError(
				exception,
				"發生例外: {Message}, 路徑: {Path}",
				exception.Message,
				httpContext.Request.Path);

			// 根據例外類型決定 HTTP 狀態碼
			var (statusCode, message) = exception switch
			{
				ArgumentException => (HttpStatusCode.BadRequest, "請求參數錯誤"),
				KeyNotFoundException => (HttpStatusCode.NotFound, "找不到指定的資源"),
				UnauthorizedAccessException => (HttpStatusCode.Forbidden, "沒有權限存取此資源"),
				InvalidOperationException => (HttpStatusCode.BadRequest, "操作無效"),
				_ => (HttpStatusCode.InternalServerError, "伺服器發生錯誤")
			};

			// 建立錯誤回應
			var problemDetails = new ProblemDetails
			{
				Status = (int)statusCode,
				Title = message,
				Detail = httpContext.RequestServices
					.GetRequiredService<IHostEnvironment>()
					.IsDevelopment()
						? exception.Message  // 開發環境顯示詳細錯誤
						: "請稍後再試或聯繫系統管理員", // 生產環境隱藏細節
				Instance = httpContext.Request.Path
			};

			httpContext.Response.StatusCode = (int)statusCode;
			await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

			return true; // 表示例外已被處理
		}
	}
}
