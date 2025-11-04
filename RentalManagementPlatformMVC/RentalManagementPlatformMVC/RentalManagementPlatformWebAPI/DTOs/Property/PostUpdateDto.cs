namespace RentalManagementPlatformWebAPI.DTOs.Property
{
	//Posts修改公告DTO(API使用)
	public class PostUpdateDto
	{
		public int PostsId { get; set; }
		public string Title { get; set; }= null!;
		public string ContactName { get; set; }= null!;
		public string Content { get; set; }= null!;
		public string? Address { get; set; }
		public string? ContactPhone { get; set; }
		public string? ContactEmail { get; set; }
		public string? ContactNote { get; set; }
		public decimal? ProposedPrice { get; set; }
		public DateTime? ExpireAt { get; set; }
		public string Status { get; set; }= null!;

	}
}
