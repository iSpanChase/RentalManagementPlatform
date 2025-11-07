using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOS;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CouponApiController : ControllerBase
    {
        private readonly ICouponApiService _couponApiService;
        // private readonly ILogger<CouponApiController> _logger;

        public CouponApiController(ICouponApiService couponApiService /*, ILogger<CouponApiController> logger*/)
        {
            _couponApiService = couponApiService;
            // _logger = logger;
        }

        [HttpPost("validate")]
		//檢查優惠券是否可用
        [Authorize]
		public async Task<IActionResult> Validate([FromBody] CouponValidationRequestDto request)
        {
            try
            {
                var result = await _couponApiService.ValidateCouponAsync(request);
                // 不論成功或失敗，都回傳 200 OK，由前端根據 IsValid 和 ErrorCode 欄位來決定行為
                return Ok(result);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "An unexpected error occurred during coupon validation.");
                return StatusCode(500, new CouponValidationResponseDto 
                { 
                    IsValid = false, 
                    ErrorCode = "ERR_SERVER_ERROR", 
                    Message = "驗證時發生未預期的錯誤。"
                });
            }
        }

        [HttpPost("redeem")]
        [Authorize]
        public async Task<IActionResult> RedeemPromoCode([FromBody] RedeemPromoCodeRequestDto request)
        {
            // TODO: 加上 [Authorize] 標籤
            var success = await _couponApiService.RedeemPromoCodeAsync(request);
            // 可以回傳更詳細的錯誤訊息
            return success ? Ok(new { message = "領取成功" }) : BadRequest(new { message = "領取失敗，可能已被領完或您已擁有。" });
        }

        [HttpPost("mark-used")]
        [Authorize]
        public async Task<IActionResult> MarkUsed([FromBody] MarkUsedRequestDto request)
        {
            // TODO: 加上 [Authorize] 標籤，並確認此 API 的呼叫權限
            var success = await _couponApiService.MarkUsedAsync(request);
            return success ? Ok(new { message = "優惠券已核銷" }) : BadRequest(new { message = "核銷失敗" });
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetPublicCoupons()
        {
            try
            {
                var coupons = await _couponApiService.GetAllCouponsAsync();
                return Ok(coupons);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "An error occurred while fetching public coupons.");
                return StatusCode(500, new { message = "讀取優惠券列表時發生錯誤。" });
            }
        }
        [HttpGet("user/{userId}")]
        [Authorize(Policy = "Coupon.View")]
		public async Task<IActionResult> GetUserCoupons(int userId)
		{
			// TODO: 加上 [Authorize] 標籤，並驗證 userId 是否為當前登入使用者
			try
			{
				var userCoupons = await _couponApiService.GetUserCouponsAsync(userId);
				return Ok(userCoupons);
			}
			catch (Exception ex)
			{
				// _logger.LogError(ex, "An error occurred while fetching coupons for user {UserId}.", userId);
				return StatusCode(500, new { message = "讀取您的優惠券時發生錯誤。" });
			}
		}

	}
}