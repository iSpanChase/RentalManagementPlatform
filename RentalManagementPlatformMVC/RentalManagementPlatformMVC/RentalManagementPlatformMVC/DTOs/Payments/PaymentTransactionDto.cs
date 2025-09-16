using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.DTOs.Payments
{
	public class PaymentTransactionDto
	{
		[Display(Name = "交易序號")]
		public int TransactionId { get; set; }

		[Display(Name = "金流商交易編號")]
		public string? ProviderTxnId { get; set; }

		[Display(Name = "回應代碼")]
		public string? ResponseCode { get; set; }

		[Display(Name = "金流商")]
		public string? Provider { get; set; }

		[Display(Name = "回應訊息")]
		public string? ResponseMessage { get; set; }

		[Display(Name = "交易參考號")]
		public string? TxnRef { get; set; }

		[Display(Name = "建立時間")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = false)]
		public DateTime? TransactionCreatedAt { get; set; }
	}
}
