using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.PointRules.ViewModels
{
    public class PointLedgerStatisticsViewModel
    {
        [Display(Name = "總獲得點數")]
        public int TotalEarnedPoints { get; set; }

        [Display(Name = "總兌換點數")]
        public int TotalRedeemedPoints { get; set; }

        [Display(Name = "總過期點數")]
        public int TotalExpiredPoints { get; set; }

        [Display(Name = "目前可用點數")]
        public int AvailablePoints { get; set; }

        [Display(Name = "即將過期點數")]
        public int SoonToExpirePoints { get; set; }

        [Display(Name = "總交易筆數")]
        public int TotalTransactions { get; set; }

        [Display(Name = "本月獲得點數")]
        public int ThisMonthEarnedPoints { get; set; }

        [Display(Name = "本月兌換點數")]
        public int ThisMonthRedeemedPoints { get; set; }

        // 顯示用屬性
        public string EarnToRedeemRatioDisplay => TotalRedeemedPoints > 0 ?
            $"{(decimal)TotalEarnedPoints / TotalRedeemedPoints:F2}" : "N/A";

        public string ExpiryRateDisplay => TotalEarnedPoints > 0 ?
            $"{(decimal)TotalExpiredPoints / TotalEarnedPoints * 100:F1}%" : "0.0%";

        public string AvailablePointsDisplay => $"{AvailablePoints:N0}";

        public string TotalEarnedPointsDisplay => $"{TotalEarnedPoints:N0}";

        public string TotalRedeemedPointsDisplay => $"{TotalRedeemedPoints:N0}";

        public string TotalExpiredPointsDisplay => $"{TotalExpiredPoints:N0}";
    }
}