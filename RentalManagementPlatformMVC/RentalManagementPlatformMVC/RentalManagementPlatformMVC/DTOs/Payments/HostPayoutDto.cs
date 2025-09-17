namespace RentalManagementPlatformMVC.DTOs.Payments
{
	public class HostPayoutDto
	{
		public int PayoutId { get; set; }
		public int? HostId { get; set; }
		public DateTime? CycleStart { get; set; }
		public DateTime? CycleEnd { get; set; }
		public decimal? AmountGross { get; set; }
		public decimal? PlatformFee { get; set; }
		public decimal? AmountNet { get; set; }
		public DateTime? PaidAt { get; set; }
		public string? Status { get; set; }
		public DateTime? CreatedAt { get; set; }

		// 關聯資料
		public string? HostName { get; set; }
	}
}
