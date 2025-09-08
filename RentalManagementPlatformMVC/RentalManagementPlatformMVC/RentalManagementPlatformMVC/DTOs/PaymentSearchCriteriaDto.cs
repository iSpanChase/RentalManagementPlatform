namespace RentalManagementPlatformMVC.DTOs
{
	public class PaymentSearchCriteriaDto
	{
		// 訂單編號
		public string? OrderNumber { get; set; }

		// 付款參考號
		public string? PaymentRef { get; set; }

		// 交易參考號
		public string? TransactionRef { get; set; }

		// 狀態
		public string? Status { get; set; }

		// 訂房姓名
		public string? GuestName { get; set; }

		// 房間名稱
		public string? RoomTitle { get; set; }

		// 日期區間
		public DateTime? PaidStartDate { get; set; }
		public DateTime? PaidEndDate { get; set; }

		// 金額區間
		public decimal? MinAmount { get; set; }
		public decimal? MaxAmount { get; set; }

		// 排序
		public string? SortBy { get; set; } = "CreatedAt";
		public bool IsDescending { get; set; } = true;
	}
}
