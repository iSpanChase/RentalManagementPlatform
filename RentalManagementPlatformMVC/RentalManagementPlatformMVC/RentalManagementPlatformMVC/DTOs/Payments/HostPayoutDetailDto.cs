namespace RentalManagementPlatformMVC.DTOs.Payments
{
	public class HostPayoutDetailDto
	{
		// 撥款基本資料 (來自 HostPayoutDto)
		public int PayoutId { get; set; }
		public int? HostId { get; set; }
		public string? HostName { get; set; }
		public DateTime? CycleStart { get; set; }
		public DateTime? CycleEnd { get; set; }
		public decimal? AmountGross { get; set; }   // 總金額
		public decimal? PlatformFee { get; set; }   // 平台手續費
		public decimal? AmountNet { get; set; }     // 實際金額
		public DateTime? PaidAt { get; set; }
		public string? Status { get; set; }
		public DateTime? CreatedAt { get; set; }

		// 關聯的撥款項目 (來自 HostPayoutItemDto)
		public List<HostPayoutItemDto> Items { get; set; } = new List<HostPayoutItemDto>();
	}
}
