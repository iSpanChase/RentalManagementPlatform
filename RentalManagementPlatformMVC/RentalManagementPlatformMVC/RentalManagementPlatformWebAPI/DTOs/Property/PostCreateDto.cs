namespace RentalManagementPlatformWebAPI.DTOs.Property
{
	//Posts新增公告DTO(API使用)
	public class PostCreateDto
	{
		public int UserId { get; set; }
		public int RegionId { get; set; }
		public string Title { get; set; }= null!;
		public string ContactName { get; set; }= null!;
		public string Content { get; set; }= null!;
		public string? Address { get; set; }
		public string? ContactPhone { get; set; }
		public string? ContactEmail { get; set; }
		public string? ContactNote { get; set; }
		public decimal? ProposedPrice { get; set; }
		public DateTime? PublishAt { get; set; }
		public DateTime? ExpireAt { get; set; }
	}
}
