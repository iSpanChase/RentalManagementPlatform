using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.ReportForm.AnomalyEvaluator
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

                    case "HostAvgRatingP30D":
                        inserted += await EvaluateHostAvgRatingP30DAsync(rule, now, ct);
                        break;

                    case "HostAvgRatingALL":
                        inserted += await EvaluateHostAvgRatingALLAsync(rule, now, ct);
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
            // 用原生 SQL 算歲數（最準確）
            var rows = await _context.Set<UserAgeRow>()
                .FromSqlRaw("""
                DECLARE @Now DATETIME2 = SYSUTCDATETIME();
                SELECT 
                  U.user_id AS TargetId,
                  DATEDIFF(YEAR, U.birth_date, @Now)
                    - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, U.birth_date, @Now), U.birth_date) > @Now THEN 1 ELSE 0 END 
                    AS Age
                FROM [USER] AS U
                WHERE U.birth_date IS NOT NULL;
            """)
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

        // ---- HostAvgRating：給你留兩個空殼（你可沿用之前的 SQL/LINQ）----
        private async Task<int> EvaluateHostAvgRatingP30DAsync(AnomalyRule rule, DateTime nowUtc, CancellationToken ct)
        {
            // TODO: 依你前面的做法把近30天的 host 平均評分聚合出來
            return 0;
        }

        private async Task<int> EvaluateHostAvgRatingALLAsync(AnomalyRule rule, DateTime nowUtc, CancellationToken ct)
        {
            // TODO: 依你前面的做法把全期間平均評分聚合出來
            return 0;
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
