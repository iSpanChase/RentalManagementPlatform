namespace RentalManagementPlatformMVC.DTOs.Payments
{
	public class HostSearchCriteriaDto
	{
		public int? HostId { get; set; }                
		public string? HostName { get; set; }           
		public DateTime? CycleStartDate { get; set; }    
		public DateTime? CycleEndDate { get; set; }     
		public DateTime? PaidStartDate { get; set; }   
		public DateTime? PaidEndDate { get; set; }       
		public decimal? MinAmount { get; set; }         
		public decimal? MaxAmount { get; set; }         
		public string? Status { get; set; }         
		public string? SortBy { get; set; }             
		public bool IsDescending { get; set; }
	}
}
