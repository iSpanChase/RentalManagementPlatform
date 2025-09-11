namespace RentalManagementPlatformMVC.DTOs
{
	public class HostPayoutItemDto
	{
		public int PayoutItemId { get; set; }
		public int? PayoutId { get; set; }  
		public int? BookingId { get; set; }  
		public string? OrderNumberSnapshot { get; set; } 
		public decimal? AmountGross { get; set; }  
		public decimal? CommissionPct { get; set; }   
		public decimal? PlatformFee { get; set; }     
		public decimal? Amount { get; set; }        
	}
}
