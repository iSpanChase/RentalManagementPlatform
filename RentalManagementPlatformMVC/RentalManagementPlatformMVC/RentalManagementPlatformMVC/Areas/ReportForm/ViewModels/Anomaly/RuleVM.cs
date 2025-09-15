using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.ReportForm.ViewModels.Anomaly
{
    public class RuleVM
    {
        public int? RuleId { get; set; } // Create 時為 null

        [Required, StringLength(100)]
        public string RuleName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string TargetType { get; set; } = "LandlordAvgRatingP30D";

        [Required, StringLength(8)]
        public string ConditionExpression { get; set; } = "<";

        [Range(typeof(decimal), "0", "9999999999")]
        public decimal? ThresholdValue { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public static class RuleDictionaries
    {
        public static readonly (string Value, string Text)[] TargetTypes =
        [
            ("HostAvgRatingP30D", "房東平均評分（近30天）"),
            ("HostAvgRatingALL", "房東平均評分（全部）"),
            ("UserAge", "使用者年齡"),
            // 之後可持續擴充其他可監控指標
        ];

        public static readonly (string Value, string Text)[] Operators =
        [
            ("<",  "<"), ("<=", "<="),
            (">",  ">"), (">=", ">="),
            ("==", "=="), ("!=", "!="),
        ];
    }
}
