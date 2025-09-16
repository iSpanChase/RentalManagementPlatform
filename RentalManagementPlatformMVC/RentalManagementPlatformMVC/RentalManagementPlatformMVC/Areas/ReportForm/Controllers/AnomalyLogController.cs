using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.ReportForm.ViewModels.Anomaly;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.ReportForm.Controllers
{
    [Area("ReportForm")]
    public class AnomalyLogController : Controller
    {
        private readonly RentalManagementPlatformSqlContext _context;
        public AnomalyLogController(RentalManagementPlatformSqlContext db)
        {
            _context = db;
        }

        // GET: /AnomalyRules
        public async Task<IActionResult> Index()
        {
            var items = await _context.AnomalyDetectionLogs
                .Join(_context.AnomalyRules, adl => adl.RuleId, ar => ar.RuleId, (adl, ar) => new Log()
                {
                    LogId = adl.LogId,
                    RuleName = ar.RuleName,
                    TargetType = ar.TargetType,
                    TargetId = adl.TargetId,
                    ConditionExpression = ar.ConditionExpression,
                    ThresholdValue = ar.ThresholdValue,
                    RealValue = adl.DetectedValue,
                    CreateTime = adl.CreatedAt
                }
                ).ToListAsync();
            return View(items);
        }
    }

    public class Log
    {
        public int LogId { get; set; }
        public string? RuleName { get; set; }
        public string? TargetType { get; set; }
        public int? TargetId { get; set; }
        public string? ConditionExpression { get; set; }
        public decimal? ThresholdValue { get; set; }
        public decimal? RealValue { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
