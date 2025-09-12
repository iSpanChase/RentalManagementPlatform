namespace RentalManagementPlatformMVC.Models
{
	public partial class PasswordResetToken
	{
		public int TokenId { get; set; }
		public int UserId { get; set; }
		public string TokenHash { get; set; } = null!;
		public DateTime ExpiresAt { get; set; }
		public DateTime UsedAt { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
