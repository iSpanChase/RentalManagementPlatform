using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.FAQ.ViewModels;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.FAQ.Controllers
{
    [Area("FAQ")]
    public class FaqCategoriesController : Controller
    {
        private readonly RentalManagementPlatformSqlContext _db;
        public FaqCategoriesController(RentalManagementPlatformSqlContext db) => _db = db;

        // 清單
        public async Task<IActionResult> Index()
        {
            var rows = await _db.FaqCategories
                .Select(c => new CategoryListRowVM
                {
                    Id = c.FaqCategoriesId,
                    Name = c.Name ?? "(未命名)",
                    ParentName = _db.FaqCategories
                        .Where(p => p.FaqCategoriesId == c.ParentId)
                        .Select(p => p.Name)
                        .FirstOrDefault(),
                    ArticleCount = _db.FaqArticles.Count(a => a.CategoryId == c.FaqCategoriesId)
                })
                .OrderBy(r => r.ParentName).ThenBy(r => r.Name)
                .ToListAsync();

            return View(rows);
        }
        // 上方：父分類清單
        public IActionResult Parents()
        {
            var parents = _db.FaqCategories
                .Where(c => c.ParentId == null)
                .OrderBy(c => c.Name)
                .ToList();
            return PartialView("_ParentsList", parents);
        }

        // 下方：指定父分類的子分類
        public IActionResult Children(int parentId)
        {
            var children = _db.FaqCategories
                .Where(c => c.ParentId == parentId)
                .OrderBy(c => c.Name)
                .ToList();
            ViewBag.ParentId = parentId;
            return PartialView("_ChildrenList", children);
        }


        private async Task<IEnumerable<SelectListItem>> BuildParentOptionsAsync(int? excludeId = null)
        {
            var q = _db.FaqCategories.AsQueryable();
            if (excludeId.HasValue) q = q.Where(c => c.FaqCategoriesId != excludeId.Value);

            return await q.OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.FaqCategoriesId.ToString(),
                    Text = c.Name!
                }).ToListAsync();
        }

        // Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new FaqCategoryFormVM
            {
                ParentOptions = await BuildParentOptionsAsync()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FaqCategoryFormVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.ParentOptions = await BuildParentOptionsAsync();
                return View(vm);
            }

            var e = new FaqCategory
            {
                Name = vm.Name,
                ParentId = vm.ParentId
            };
            _db.FaqCategories.Add(e);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var e = await _db.FaqCategories.FindAsync(id);
            if (e == null) return NotFound();

            var vm = new FaqCategoryFormVM
            {
                FaqCategoriesId = e.FaqCategoriesId,
                Name = e.Name ?? "",
                ParentId = e.ParentId,
                ParentOptions = await _db.FaqCategories
                    .Where(c => c.FaqCategoriesId != id)
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.FaqCategoriesId.ToString(),
                        Text = c.Name!
                    }).ToListAsync()
            };

            // 你用 modal 載入 → 回 Partial；直接瀏覽也 OK
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_Edit", vm);
            return View("Edit", vm); // 可選：提供全頁版 Edit.cshtml
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FaqCategoryFormVM m)
        {
            if (m.FaqCategoriesId == null) return BadRequest("缺少ID");

            var e = await _db.FaqCategories.FindAsync(m.FaqCategoriesId.Value);
            if (e == null) return NotFound();

            // 這裡可再做「避免自指向」等驗證
            e.Name = m.Name;
            e.ParentId = m.ParentId; // 可為 null 代表升級為父分類

            await _db.SaveChangesAsync();
            return Json(new { ok = true, parentId = e.ParentId ?? 0 });
        }

        // Delete
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var e = await _db.FaqCategories.FindAsync(id);
            if (e == null) return NotFound();

            var vm = new FaqCategoryDeleteVM
            {
                Id = e.FaqCategoriesId,
                Name = e.Name ?? "",
                HasChildren = await _db.FaqCategories.AnyAsync(c => c.ParentId == id),
                HasArticles = await _db.FaqArticles.AnyAsync(a => a.CategoryId == id)
            };
            return View(vm);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasChildren = await _db.FaqCategories.AnyAsync(c => c.ParentId == id);
            var hasArticles = await _db.FaqArticles.AnyAsync(a => a.CategoryId == id);
            if (hasChildren || hasArticles)
            {
                TempData["Error"] = "此分類仍有子分類或文章，請先移動/刪除後再嘗試。";
                return RedirectToAction(nameof(Delete), new { id });
            }

            var e = await _db.FaqCategories.FindAsync(id);
            if (e != null) _db.FaqCategories.Remove(e);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
