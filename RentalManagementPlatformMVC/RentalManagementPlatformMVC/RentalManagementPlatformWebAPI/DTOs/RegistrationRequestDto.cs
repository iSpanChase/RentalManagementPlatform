namespace RentalManagementPlatformWebAPI.DTOs
{
	public class RegistrationRequestDto
	{
		public string RoleCode { get; set; } = null!; // LANDLORD/TENANT/VENDOR/ADMIN
		public string Email { get; set; } = null!;
		public string PasswordHash { get; set; } = null!;
		public string Name { get; set; } = null!;
		public string Username { get; set; } = null!;
		public string Phone { get; set; } = "";
		// 依角色擴充欄位（範例）
		public string? CompanyName { get; set; }
		public string? TaxId { get; set; }
		public string? NationalIdTail { get; set; }
	}
}
