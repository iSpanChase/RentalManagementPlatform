namespace RentalManagementPlatformMVC.Areas.Management.DTOS
{
	public class PostQueryDto
	{
		public int PostsId { get; set; }

		public int UserId { get; set; }

		public string Name { get; set; } = null!;//user的名字(user中提取)

		public int RegionId { get; set; }

		public string DistrictName { get; set; } = null!;//DistrictName行政區名稱(District中提取)

		public string Title { get; set; } = null!;

		public string ContactName { get; set; } = null!;

		public string Content { get; set; } = null!;

		public string? Address { get; set; }

		public string? ContactPhone { get; set; }

		public string? ContactEmail { get; set; }

		public string? ContactNote { get; set; }

		public DateTime? PublishAt { get; set; }

		public DateTime? ExpireAt { get; set; }

		public int? Views { get; set; }

		public string Status { get; set; } = null!;

		public DateTime CreatedAt { get; set; }

		public DateTime UpdatedAt { get; set; }

		public DateTime? DeletedAt { get; set; }

		public decimal? ProposedPrice { get; set; }
	}
}
