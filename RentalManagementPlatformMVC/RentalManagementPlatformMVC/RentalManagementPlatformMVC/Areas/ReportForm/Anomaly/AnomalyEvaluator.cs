using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.ReportForm.Anomaly
{
    public class AnomalyEvaluator : IAnomalyEvaluator
    {
        private readonly RentalManagementPlatformSqlContext _context;
        private readonly ILogger<AnomalyEvaluator> _logger;

        public AnomalyEvaluator(RentalManagementPlatformSqlContext db, ILogger<AnomalyEvaluator> logger)
        {
            _context = db;
            _logger = logger;
        }

        public async Task<int> EvaluateOnceAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var rules = await _context.AnomalyRules
                .Where(r => r.IsActive.Value)
                .ToListAsync(ct);

            int inserted = 0;

            foreach (var rule in rules)
            {
                switch (rule.TargetType)
                {
                    case "UserAge":
                        inserted += await EvaluateUserAgeAsync(rule, now, ct);
                        break;

                    default:
                        _logger.LogWarning("Unknown TargetType: {tt}", rule.TargetType);
                        break;
                }
            }

            return inserted;
        }

        // ---- UserAge: 從生日換算年齡，逐一比對 ----
        private async Task<int> EvaluateUserAgeAsync(AnomalyRule rule, DateTime nowUtc, CancellationToken ct)
        {
            var today = DateTime.UtcNow.AddHours(8).Date;

            // 用原生 SQL 算歲數（最準確）
            var rows = await _context.Users
                .Where(u => u.BirthDate != null)
                .Select(u => new UserAgeRow
                {
                    TargetId = u.UserId,
                    Age = (today.Year - u.BirthDate.Year)
                                    - ((u.BirthDate.Month > today.Month) ||
                                            (u.BirthDate.Month == today.Month && u.BirthDate.Day > today.Day)
                                        ? 1 : 0
                                    )
                })
                .AsNoTracking()
                .ToListAsync(ct);

            int inserted = 0;

            foreach (var r in rows)
            {
                var value = (decimal)r.Age;
                bool isAbnormal = Compare(value, rule.ConditionExpression, rule.ThresholdValue.Value);

                // 取最近一筆事件（沒有就 null）
                var last = await _context.AnomalyDetectionLogs
                    .Where(x => x.RuleId == rule.RuleId && x.TargetId == r.TargetId)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => x.EventType)
                    .FirstOrDefaultAsync(ct);

                if (isAbnormal)
                {
                    if (!string.Equals(last, "ALERT", StringComparison.OrdinalIgnoreCase))
                    {
                        _context.AnomalyDetectionLogs.Add(new AnomalyDetectionLog
                        {
                            RuleId = rule.RuleId,
                            TargetId = r.TargetId,
                            DetectedValue = value,
                            ExpectedValue = rule.ThresholdValue,
                            CreatedAt = nowUtc,
                            EventType = "ALERT"
                        });
                        inserted++;
                    }
                }
                else
                {
                    if (string.Equals(last, "ALERT", StringComparison.OrdinalIgnoreCase))
                    {
                        _context.AnomalyDetectionLogs.Add(new AnomalyDetectionLog
                        {
                            RuleId = rule.RuleId,
                            TargetId = r.TargetId,
                            DetectedValue = value,
                            ExpectedValue = rule.ThresholdValue,
                            CreatedAt = nowUtc,
                            EventType = "RECOVER"
                        });
                        inserted++;
                    }
                }
            }

            if (inserted > 0) await _context.SaveChangesAsync(ct);
            return inserted;
        }

        private static bool Compare(decimal value, string op, decimal threshold) => op switch
        {
            "<" => value < threshold,
            "<=" => value <= threshold,
            ">" => value > threshold,
            ">=" => value >= threshold,
            "==" => value == threshold,
            "!=" => value != threshold,
            _ => false
        };

        // Keyless 映射（年齡查詢結果）
        public class UserAgeRow
        {
            public int TargetId { get; set; }
            public int Age { get; set; }
        }
    }
}
