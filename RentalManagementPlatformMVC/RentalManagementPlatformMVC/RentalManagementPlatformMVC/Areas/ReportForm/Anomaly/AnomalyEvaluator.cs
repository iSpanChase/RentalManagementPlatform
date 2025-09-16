using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.ReportForm.Anomaly
{
    public class AnomalyEvaluator : IAnomalyEvaluator
    {
        private readonly RentalManagementPlatformSqlContext _context;
        private readonly AnomalyNotifier _notifier;
        public AnomalyEvaluator(RentalManagementPlatformSqlContext db, AnomalyNotifier notifier)
        {
            _context = db;
            _notifier = notifier;
        }

        public async Task<int> EvaluateOnceAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow.AddHours(8);
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
                }
            }

            return inserted;
        }

        // ---- UserAge: 從生日換算年齡，逐一比對 ----
        private async Task<int> EvaluateUserAgeAsync(AnomalyRule rule, DateTime now, CancellationToken ct)
        {
            // 用原生 SQL 算歲數（最準確）
            var rows = await _context.Users
                .Where(u => u.BirthDate != null)
                .Select(u => new UserAgeRow
                {
                    TargetId = u.UserId,
                    Age = (now.Year - u.BirthDate.Year)
                                    - ((u.BirthDate.Month > now.Month) ||
                                            (u.BirthDate.Month == now.Month && u.BirthDate.Day > now.Day)
                                        ? 1 : 0
                                    )
                })
                .AsNoTracking()
                .ToListAsync(ct);

            int inserted = 0;
            var eventsToBroadcast = new List<object>();
            foreach (var r in rows)
            {
                var value = (decimal)r.Age;

                // 取最近一筆事件（沒有就 null）
                string? last = await _context.AnomalyDetectionLogs
                    .Where(x => x.RuleId == rule.RuleId && x.TargetId == r.TargetId)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => x.EventType)
                    .FirstOrDefaultAsync(ct);

                bool isAbnormal = Compare(value, rule.ConditionExpression, rule.ThresholdValue.Value);
                bool isAbnormalBefore = string.Equals(last, "ALERT", StringComparison.OrdinalIgnoreCase);
                if (isAbnormal != isAbnormalBefore)
                {
                    inserted++;
                    _context.AnomalyDetectionLogs.Add(new AnomalyDetectionLog
                    {
                        RuleId = rule.RuleId,
                        TargetId = r.TargetId,
                        DetectedValue = value,
                        ExpectedValue = rule.ThresholdValue,
                        CreatedAt = now,
                        EventType = isAbnormal ? "ALERT" : "RECOVER",
                    });

                    if (isAbnormal)
                    {
                        eventsToBroadcast.Add(new
                        {
                            type = isAbnormal ? "ALERT" : "RECOVER",
                            ruleId = rule.RuleId,
                            ruleName = rule.RuleName,     // 親和一點
                            targetId = r.TargetId,
                            value,
                            expected = rule.ThresholdValue,
                            at = now
                        });
                    }
                }
            }

            if (inserted > 0)
            {
                await _context.SaveChangesAsync(ct);
                foreach (var ev in eventsToBroadcast)
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(ev);
                    await _notifier.BroadcastAsync(json);
                }
            }
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
