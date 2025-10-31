namespace RentalManagementPlatformWebAPI.DTOs.Property
{
	//Posts公告列表DTO(API使用)
	public class PostListDto
	{
		public int PostsId { get; set; }
		public string Title { get; set; } = null!;
		public string ContactName { get; set; } = null!;
		public string Status { get; set; } = null!;
		public int Views { get; set; }
		public DateTime? PublishAt { get; set; }
		public DateTime? ExpireAt { get; set; }
	}
}
