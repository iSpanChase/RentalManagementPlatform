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
            var items = await (
                 from adl in _context.AnomalyDetectionLogs
                 join maxCreated in (
                     from log in _context.AnomalyDetectionLogs
                     group log by log.TargetId into g
                     select new { TargetId = g.Key, CreatedAt = g.Max(x => x.CreatedAt) }
                 ) on new { adl.TargetId, adl.CreatedAt } equals new { maxCreated.TargetId, maxCreated.CreatedAt }
                 join ar in _context.AnomalyRules on adl.RuleId equals ar.RuleId
                 select new Log
                 {
                     RuleName = ar.RuleName,
                     TargetType = ar.TargetType,
                     TargetId = adl.TargetId,
                     ConditionExpression = ar.ConditionExpression,
                     ThresholdValue = ar.ThresholdValue,
                     RealValue = adl.DetectedValue,
                     CreateTime = adl.CreatedAt,
                     Solved = adl.EventType != "ALERT"
                 }
             ).OrderBy(x=>x.Solved).ThenByDescending(x=>x.CreateTime).ThenBy(x => x.TargetId).ToListAsync();

            ViewBag.TargetTypeMap = RuleDictionaries.TargetTypes.ToDictionary(x => x.Value, x => x.Text);
            return View(items);
        }
    }

    public class Log
    {
        public string? RuleName { get; set; }
        public string? TargetType { get; set; }
        public int? TargetId { get; set; }
        public string? ConditionExpression { get; set; }
        public decimal? ThresholdValue { get; set; }
        public decimal? RealValue { get; set; }
        public DateTime? CreateTime { get; set; }
        public bool Solved { get; set; }
    }
}
