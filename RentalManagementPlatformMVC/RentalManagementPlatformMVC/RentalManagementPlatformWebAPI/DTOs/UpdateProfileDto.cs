namespace RentalManagementPlatformWebAPI.DTOs
{
	public class UpdateProfileDto
	{
		// NOT NULL 欄位（請依 DB 規則）
		public string Name { get; set; } = null!;
		public string Gender { get; set; } = null!;
		// 後端：接 yyyy-MM-dd，model binder 會解析為 DateTime
		public DateTime BirthDate { get; set; }
		public string Address { get; set; } = null!;

		// 可為 NULL 的欄位
		public string? Phone { get; set; }
		public int? Point { get; set; }
		public string? ProfileImageUrl { get; set; }
	}
}
