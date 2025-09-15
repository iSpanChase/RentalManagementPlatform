using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.PointRules.ViewModels
{
    public class PointLedgerSearchCriteriaViewModel
    {
        [Display(Name = "客戶ID")]
        public int? GuestId { get; set; }

        [Display(Name = "客戶姓名")]
        [StringLength(50, ErrorMessage = "客戶姓名長度不能超過50個字符")]
        public string? GuestName { get; set; }

        [Display(Name = "訂單號碼")]
        [StringLength(50, ErrorMessage = "訂單號碼長度不能超過50個字符")]
        public string? OrderNumberSnapshot { get; set; }

        [Display(Name = "點數類型")]
        public string? Type { get; set; }

        [Display(Name = "點數(從)")]
        [Range(int.MinValue, int.MaxValue, ErrorMessage = "請輸入有效的點數")]
        public int? PointsFrom { get; set; }

        [Display(Name = "點數(到)")]
        [Range(int.MinValue, int.MaxValue, ErrorMessage = "請輸入有效的點數")]
        public int? PointsTo { get; set; }

        [Display(Name = "發生時間(從)")]
        [DataType(DataType.DateTime)]
        public DateTime? OccurredAtFrom { get; set; }

        [Display(Name = "發生時間(到)")]
        [DataType(DataType.DateTime)]
        public DateTime? OccurredAtTo { get; set; }

        [Display(Name = "到期時間(從)")]
        [DataType(DataType.DateTime)]
        public DateTime? ExpiresAtFrom { get; set; }

        [Display(Name = "到期時間(到)")]
        [DataType(DataType.DateTime)]
        public DateTime? ExpiresAtTo { get; set; }

        [Display(Name = "備註")]
        [StringLength(500, ErrorMessage = "備註長度不能超過500個字符")]
        public string? Note { get; set; }

        [Display(Name = "是否過期")]
        public bool? IsExpired { get; set; }

        // 排序相關屬性
        [Display(Name = "排序欄位")]
        public string? SortBy { get; set; }

        [Display(Name = "降冪排序")]
        public bool IsDescending { get; set; } = true;
    }
}