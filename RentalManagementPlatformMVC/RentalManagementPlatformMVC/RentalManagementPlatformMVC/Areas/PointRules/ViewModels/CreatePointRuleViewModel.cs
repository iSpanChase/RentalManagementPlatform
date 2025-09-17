using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.PointRules.ViewModels
{
    public class CreatePointRuleViewModel
    {
        [Required(ErrorMessage = "點數獲得必填")]
        [Range(0.01, double.MaxValue, ErrorMessage = "點數獲得必須大於0")]
        [Display(Name = "每台幣獲得點數")]
        public decimal EarnRatePerNtd { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "每訂單最高點數不能為負數")]
        [Display(Name = "每訂單最高點數")]
        public int? MaxPointsPerOrder { get; set; }

        [Range(1, 120, ErrorMessage = "點數有效期限必須在1-120個月之間")]
        [Display(Name = "點數有效月數")]
        public int? ExpiryMonths { get; set; }

        [Required(ErrorMessage = "點數使用必填")]
        [Range(1, double.MaxValue, ErrorMessage = "點數使用必須大於0")]
        [Display(Name = "每點兌換台幣")]
        public decimal RedeemRateNtdPerPt { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "生效日期")]
        public DateTime? ActiveFrom { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "失效日期")]
        public DateTime? ActiveTo { get; set; }
    }
}