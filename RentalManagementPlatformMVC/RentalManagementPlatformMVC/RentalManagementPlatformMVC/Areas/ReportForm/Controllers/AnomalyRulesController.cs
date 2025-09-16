using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.ReportForm.ViewModels.Anomaly;
using RentalManagementPlatformMVC.Models;
using System.Threading.Tasks;

namespace RentalManagementPlatformMVC.Areas.ReportForm.Controllers
{
    [Area("ReportForm")]
    public class AnomalyRulesController : Controller
    {
        private readonly RentalManagementPlatformSqlContext _context;
        public AnomalyRulesController(RentalManagementPlatformSqlContext db)
        {
            _context = db;
        }

        // GET: /AnomalyRules
        public async Task<IActionResult> Index()
        {
            var items = await _context.AnomalyRules
                .OrderByDescending(x => x.IsActive)
                .ThenBy(x => x.TargetType)
                .ThenBy(x => x.RuleName)
                .ToListAsync();

            ViewBag.TargetTypeMap = RuleDictionaries.TargetTypes.ToDictionary(x => x.Value, x => x.Text);
            return View(items);
        }

        // GET: /AnomalyRules/Create
        public IActionResult Create()
        {
            FillSelectLists();
            return View(new RuleVM());
        }

        // POST: /AnomalyRules/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RuleVM vm)
        {
            if (!ModelState.IsValid) { FillSelectLists(); return View(vm); }

            var entity = new AnomalyRule
            {
                RuleName = vm.RuleName.Trim(),
                TargetType = vm.TargetType,
                ConditionExpression = vm.ConditionExpression,
                ThresholdValue = vm.ThresholdValue,
                IsActive = vm.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnomalyRules.Add(entity);
            await _context.SaveChangesAsync();

            TempData["msg"] = "已新增規則。";
            return RedirectToAction(nameof(Index));
        }

        // GET: /AnomalyRules/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var e = await _context.AnomalyRules.FindAsync(id);
            if (e == null) return NotFound();

            var vm = new RuleVM
            {
                RuleId = e.RuleId,
                RuleName = e.RuleName,
                TargetType = e.TargetType,
                ConditionExpression = e.ConditionExpression,
                ThresholdValue = e.ThresholdValue,
                IsActive = e.IsActive.Value
            };
            FillSelectLists();
            return View(vm);
        }

        // POST: /AnomalyRules/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var e = await _context.AnomalyRules.FindAsync(id);
            if (e == null)
                return NotFound();

            if (!await IsEditable(id))
            {
                // 回傳 Index 頁面，但附帶一個「需要確認刪除」的 flag
                TempData["ShowDeleteConfirm"] = true;
                TempData["DeleteId"] = id;
                TempData["RuleName"] = e.RuleName;
                return RedirectToAction(nameof(Index));
            }

            _context.AnomalyRules.Remove(e);
            await _context.SaveChangesAsync();
            TempData["msg"] = "已刪除規則。";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var e = await _context.AnomalyRules.FindAsync(id);
            if (e != null)
            {
                _context.AnomalyRules.Remove(e);
                _context.RemoveRange(_context.AnomalyDetectionLogs.Where(x => x.RuleId == id));
                await _context.SaveChangesAsync();
                TempData["msg"] = "已刪除規則。";
            }
            return RedirectToAction(nameof(Index));
        }

        async Task<bool> IsEditable(int id)
        {
            return !await _context.AnomalyDetectionLogs.Where(x=>x.RuleId == id).AnyAsync();
        }

        // POST: /AnomalyRules/Toggle/5 （啟用/停用）
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var e = await _context.AnomalyRules.FindAsync(id);
            if (e == null) return NotFound();

            e.IsActive = e.IsActive ?? false;

            e.IsActive = !e.IsActive;
            await _context.SaveChangesAsync();
            TempData["msg"] = e.IsActive.Value ? "已啟用。" : "已停用。";
            return RedirectToAction(nameof(Index));
        }

        private void FillSelectLists()
        {
            ViewBag.TargetTypes = RuleDictionaries.TargetTypes
                .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = x.Value, Text = x.Text })
                .ToList();

            ViewBag.Operators = RuleDictionaries.Operators
                .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = x.Value, Text = x.Text })
                .ToList();
        }
    }
}
