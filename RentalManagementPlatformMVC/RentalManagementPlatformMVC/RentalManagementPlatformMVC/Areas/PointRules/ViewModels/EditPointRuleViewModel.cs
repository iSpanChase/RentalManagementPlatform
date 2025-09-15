using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.PointRules.ViewModels
{
    public class EditPointRuleViewModel
    {
        [Required]
        public int RuleId { get; set; }

        [Required(ErrorMessage = "點數獲得必填")]
        [Range(0.01, double.MaxValue, ErrorMessage = "點數獲得必須大於0")]
        [Display(Name = "點數獲得")]
        public decimal EarnRatePerNtd { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "單筆上限不能為負數")]
        [Display(Name = "單筆上限")]
        public int? MaxPointsPerOrder { get; set; }

        [Range(1, 120, ErrorMessage = "有效期限必須在1-120個月之間")]
        [Display(Name = "有效期限")]
        public int? ExpiryMonths { get; set; }

        [Required(ErrorMessage = "點數使用必填")]
        [Range(0.01, double.MaxValue, ErrorMessage = "點數使用必須大於0")]
        [Display(Name = "點數使用")]
        public decimal RedeemRateNtdPerPt { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "開始日期")]
        public DateTime? ActiveFrom { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "結束日期")]
        public DateTime? ActiveTo { get; set; }
    }
}