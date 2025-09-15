using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.FAQ.Controllers;
using RentalManagementPlatformMVC.Models;
using System.Text.RegularExpressions;

namespace RentalManagementPlatform.Areas.FAQ.Controllers;

[Area("FAQ")]
[Route("FAQ/Admin/[controller]/[action]")]
public class ArticlesController : Controller
{
    private readonly RentalManagementPlatformSqlContext _db;
    public ArticlesController(RentalManagementPlatformSqlContext db) => _db = db;
    // ---- tools: 產生唯一 slug（解 slug UNIQUE 的 500） ----
    private static string ToSlug(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Guid.NewGuid().ToString("n").Substring(0, 8);

        var s = text.Trim().ToLowerInvariant();
        s = Regex.Replace(s, @"\s+", "-");
        s = Regex.Replace(s, @"[^a-z0-9\-]", "");
        return string.IsNullOrWhiteSpace(s) ? Guid.NewGuid().ToString("n").Substring(0, 8) : s;
    }

    private async Task<string> EnsureUniqueSlugAsync(string baseSlug)
    {
        var slug = baseSlug;
        var n = 2;
        while (await _db.FaqArticles.AnyAsync(x => x.Slug == slug))
            slug = $"{baseSlug}-{n++}";
        return slug;
    }
    // ---- DTO ----
    public class UpsertArticleDto
    {
        public int? ArticleId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; } = "";
        public string? Content { get; set; }
        public string Status { get; set; } = "draft"; // draft|published|archived
    }

    // GET: /FAQ/Admin/Articles/List
    [HttpGet]
    [Produces("application/json")]
    public async Task<IActionResult> List()
    {
        try
        {
            var list = await _db.FaqArticles
                .AsNoTracking()
                .Include(a => a.Category) // 有沒有載入都沒關係，下面用 null-safe
                .Select(a => new
                {
                    ArticleId = a.FaqArticlesId,
                    CategoryId = a.CategoryId,
                    CategoryName = a.Category != null ? a.Category.Name : null, // ← null-safe，避免 NRE 500
                    Title = a.Title,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .OrderBy(a => a.CategoryName)
                .ThenByDescending(a => a.UpdatedAt) // 同分類內最新的在上
                .ToListAsync();

            return Json(ApiResponse<object>.Ok(list));
        }
        catch (Exception ex)
        {
            return Json(ApiResponse<object>.Fail($"伺服器錯誤：{ex.Message}"));
        }
    }

    // GET: /FAQ/Admin/Articles/Get?id=123
    [HttpGet]
    [Produces("application/json")]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var a = await _db.FaqArticles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.FaqArticlesId == id);

            if (a == null) return Json(ApiResponse<object>.Fail("找不到文章"));

            return Json(ApiResponse<object>.Ok(new
            {
                ArticleId = a.FaqArticlesId,
                a.CategoryId,
                a.Title,
                a.Content,
                a.Status
            }));
        }
        catch (Exception ex)
        {
            return Json(ApiResponse<object>.Fail($"伺服器錯誤：{ex.Message}"));
        }
    }

    // POST: /FAQ/Admin/Articles/Upsert
    [HttpPost]
    [Produces("application/json")]
    // [ValidateAntiForgeryToken] // 確認流程 OK 再打開
    public async Task<IActionResult> Upsert([FromBody] UpsertArticleDto? dto)
    {
        try
        {
            if (dto == null) return Json(ApiResponse<object>.Fail("請求內容為空"));
            if (dto.CategoryId <= 0) return Json(ApiResponse<object>.Fail("請選擇子分類"));
            if (string.IsNullOrWhiteSpace(dto.Title)) return Json(ApiResponse<object>.Fail("標題不可空白"));

            // FK 防呆：分類必須存在
            var catExists = await _db.FaqCategories.AnyAsync(c => c.FaqCategoriesId == dto.CategoryId);
            if (!catExists) return Json(ApiResponse<object>.Fail("子分類不存在"));

            if (dto.ArticleId is null)
            {
                // === 新增 ===（解 UNIQUE: slug）
                var baseSlug = ToSlug(dto.Title);
                var slug = await EnsureUniqueSlugAsync(baseSlug);

                var e = new FaqArticle
                {
                    CategoryId = dto.CategoryId,
                    Title = dto.Title.Trim(),
                    Content = dto.Content,
                    Status = dto.Status,
                    Slug = slug,               // ★ 一定塞，避免 NULL 撞 UNIQUE
                    CreatedAt = DateTime.UtcNow
                };
                _db.FaqArticles.Add(e);
                await _db.SaveChangesAsync();
                return Json(ApiResponse<object>.Ok(new { e.FaqArticlesId }, "已新增"));
            }
            else
            {
                // === 更新 ===
                var e = await _db.FaqArticles.FindAsync(dto.ArticleId.Value);
                if (e == null) return Json(ApiResponse<object>.Fail("找不到文章"));

                e.CategoryId = dto.CategoryId;
                e.Title = dto.Title.Trim();
                e.Content = dto.Content;
                e.Status = dto.Status;

                // 舊資料若 slug 是 NULL，也補一個（避免之後再撞 UNIQUE）
                if (string.IsNullOrWhiteSpace(e.Slug))
                    e.Slug = await EnsureUniqueSlugAsync(ToSlug(e.Title));

                e.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return Json(ApiResponse<object>.Ok(new { e.FaqArticlesId }, "已更新"));
            }
        }
        catch (DbUpdateException ex)
        {
            // 會把 UNIQUE/FK 的真正錯誤回給前端（JSON），不會再是 HTML 500
            return Json(ApiResponse<object>.Fail($"資料庫錯誤：{ex.InnerException?.Message ?? ex.Message}"));
        }
        catch (Exception ex)
        {
            return Json(ApiResponse<object>.Fail($"伺服器錯誤：{ex.Message}"));
        }
    }



    // DELETE: /FAQ/Admin/Articles/Delete?id=123
    [HttpDelete]
    [Produces("application/json")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            // 連動刪回饋（你也可以在 FK 上設定 Cascade；這裡保險起見程式處理）
            var fb = await _db.FaqFeedbacks.Where(f => f.ArticleId == id).ToListAsync();
            if (fb.Any()) _db.FaqFeedbacks.RemoveRange(fb);

            var a = await _db.FaqArticles.FirstOrDefaultAsync(x => x.FaqArticlesId == id);
            if (a == null) return Json(ApiResponse<object>.Fail("文章不存在"));

            _db.FaqArticles.Remove(a);
            await _db.SaveChangesAsync();
            return Json(ApiResponse<object>.Ok(new { id }, "已刪除"));
        }
        catch (DbUpdateException ex)
        {
            return Json(ApiResponse<object>.Fail($"資料庫錯誤：{ex.InnerException?.Message ?? ex.Message}"));
        }
        catch (Exception ex)
        {
            return Json(ApiResponse<object>.Fail($"伺服器錯誤：{ex.Message}"));
        }
    }

    // 提供給前端載入「所有子分類」下拉
    [HttpGet]
    public async Task<IActionResult> AllChildrenForDropdown()
    {
        var data = await _db.FaqCategories
            .Where(c => c.ParentId != null)
            .Include(c => c.Parent!)
            .OrderBy(c => c.Parent!.Name).ThenBy(c => c.Name)
            .Select(c => new { c.FaqCategoriesId, Text = c.Parent!.Name + " / " + c.Name })
            .ToListAsync();
        return Json(ApiResponse<object>.Ok(data));
    }

    [HttpGet]
    public async Task<IActionResult> AllParentsForDropdown()
    {
        var data = await _db.FaqCategories
            .Where(c => c.ParentId == null)
            .OrderBy(c => c.Name)
            .Select(c => new { c.FaqCategoriesId, c.Name })
            .ToListAsync();
        return Json(ApiResponse<object>.Ok(data));
    }
}
