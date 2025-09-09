namespace RentalManagementPlatformMVC.DTOs
{
	public class PaymentTransactionDto
	{
		public int TransactionId { get; set; }
		public string? ProviderTxnId { get; set; }
		public string? ResponseCode { get; set; }
		public string? Provider { get; set; }
		public string? ResponseMessage { get; set; }
		public string? TxnRef { get; set; }
		public DateTime? TransactionCreatedAt { get; set; }
	}
}
