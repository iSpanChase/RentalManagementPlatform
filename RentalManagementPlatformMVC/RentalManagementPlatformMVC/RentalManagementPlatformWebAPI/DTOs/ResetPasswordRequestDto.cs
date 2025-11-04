namespace RentalManagementPlatformWebAPI.DTOs
{
	public class ResetPasswordRequestDto
	{
		public string Token { get; set; } = default!;      // 必填
		public string NewPassword { get; set; } = default!; // 必填
		public string? Email { get; set; }                  // ← 可選，用於舊 token 的保底
	}
}
