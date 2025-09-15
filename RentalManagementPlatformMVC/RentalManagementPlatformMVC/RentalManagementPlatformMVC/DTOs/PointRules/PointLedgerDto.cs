using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.DTOs.PointRules
{
    public class PointLedgerDto
    {
        public int LedgerId { get; set; }

        public int? GuestId { get; set; }

        public int? BookingId { get; set; }

        [Display(Name = "訂單號碼")]
        public string? OrderNumberSnapshot { get; set; }

        [Display(Name = "點數類型")]
        public string? Type { get; set; }

        [Display(Name = "點數")]
        public int? Points { get; set; }

        [Display(Name = "發生時間")]
        public DateTime? OccurredAt { get; set; }

        [Display(Name = "到期時間")]
        public DateTime? ExpiresAt { get; set; }

        [Display(Name = "備註")]
        public string? Note { get; set; }

        // 導覽屬性
        [Display(Name = "房客姓名")]
        public string? GuestName { get; set; }

        // 顯示用屬性
        public string TypeDisplay => Type switch
        {
            "EARN" => "獲得",
            "REDEEM" => "兌換",
            "EXPIRE" => "過期",
            "REFUND" => "退款",
            _ => Type ?? "未知"
        };
    }
}