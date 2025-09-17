using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Auth.ViewModels
{
    public class RegisterVm
    {
		[Required, MaxLength(512)]
		[Display(Name = "帳號")]
		public string Username { get; set; } = null!;

		[Required, EmailAddress, MaxLength(512)]
		[Display(Name = "Email")]
		public string Email { get; set; } = null!;

		[Required, MaxLength(512)]
		[Display(Name = "姓名")]
		public string Name { get; set; } = null!;

		[Required, MinLength(6)]
		[DataType(DataType.Password)]
		[Display(Name = "密碼")]
		public string Password { get; set; } = null!;

		// 新增欄位
		[Required]
		[Display(Name = "性別")]
		public string Gender { get; set; } = null!; // 存「男」或「女」

		[Required]
		[DataType(DataType.Date)]
		[Display(Name = "生日")]
		public DateTime BirthDate { get; set; } = DateTime.Today; // 預設今天

		[Required, Phone]
		[Display(Name = "電話")]
		public string Phone { get; set; } = null!;

		[Required]
		[MaxLength(512)]
		[Display(Name = "地址")]
		public string Address { get; set; } = null!;

		[Url]
		[Display(Name = "頭像圖片 URL（可省略）")]
		public string? ProfileImageurl { get; set; }
	}
}
