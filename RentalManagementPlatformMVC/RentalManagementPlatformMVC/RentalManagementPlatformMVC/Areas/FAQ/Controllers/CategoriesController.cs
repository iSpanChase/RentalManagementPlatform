using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.FAQ.Controllers;
using RentalManagementPlatformMVC.Models;
using System.Text.RegularExpressions;

namespace RentalManagementPlatform.Areas.FAQ.Controllers;

[Area("FAQ")]
[Authorize]
[Route("FAQ/Admin/[controller]/[action]")]
public class CategoriesController : Controller
{
    private readonly RentalManagementPlatformSqlContext _db;
    public CategoriesController(RentalManagementPlatformSqlContext db) => _db = db;


    // 父分類清單
    [HttpGet]
    public async Task<IActionResult> Parents()
    {
        var data = await _db.FaqCategories
            .Where(c => c.ParentId == null)
            .OrderBy(c => c.FaqCategoriesId)
            .Select(c => new { c.FaqCategoriesId, c.Name })
            .ToListAsync();
        return Json(ApiResponse<object>.Ok(data));
    }

    // 子分類清單（可帶 parentId；不帶則全部）
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Children(int? parentId)
    {
        var q = _db.FaqCategories.AsQueryable().Where(c => c.ParentId != null);
        if (parentId.HasValue) q = q.Where(c => c.ParentId == parentId.Value);

        var data = await q
            .Include(c => c.Parent!)
            .OrderBy(c => c.FaqCategoriesId)
            .Select(c => new {
                CategoryId = c.FaqCategoriesId,
                ParentId = c.ParentId,
                ParentName = c.Parent != null ? c.Parent.Name : null,
                Name = c.Name,
                SortOrder = c.SortOrder
            })
            // ① 想照顯示名稱排：
            .OrderBy(c => c.ParentName)
            .ThenBy(c => c.Name)
            // ② 如果你有維護 SortOrder，改成這兩行會更可控：
            // .OrderBy(c => c.ParentId)
            // .ThenBy(c => c.SortOrder ?? int.MaxValue)
            .ToListAsync();


        return Json(ApiResponse<object>.Ok(data));
    }

    /// slug unique 問題
    private static string ToSlug(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Guid.NewGuid().ToString("n").Substring(0, 8);

        var s = text.Trim().ToLowerInvariant();
        s = Regex.Replace(s, @"\s+", "-");        // 空白 → -
        s = Regex.Replace(s, @"[^a-z0-9\-]", "");  // 只留 a-z, 0-9, -
        return string.IsNullOrWhiteSpace(s)
            ? Guid.NewGuid().ToString("n").Substring(0, 8)
            : s;
    }

    private async Task<string> EnsureUniqueSlugAsync(string baseSlug)
    {
        var slug = baseSlug;
        var n = 2;
        while (await _db.FaqCategories.AnyAsync(c => c.Slug == slug))
            slug = $"{baseSlug}-{n++}";
        return slug;
    }

    // 新增或更新 父/子分類
    public class UpsertCategoryDto
    {
        public int? CategoryId { get; set; }
        public string? Name { get; set; }
        public int? ParentId { get; set; }  // 子分類才會用到；父分類一律忽略
    }

    [HttpPost]
    [Authorize]
    //[ValidateAntiForgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Upsert([FromBody] UpsertCategoryDto dto)
    {
        try
        {
            // 0) 基本檢查（防空、防空白）
            if (dto is null) return Json(ApiResponse<object>.Fail("請求內容為空"));
            if (string.IsNullOrWhiteSpace(dto.Name)) return Json(ApiResponse<object>.Fail("名稱不可空白"));

            // 1) 決定這次是「父」還是「子」
            // 規則：ParentId 有值 => 子分類；ParentId 為 null => 父分類
            var isChild = dto.ParentId.HasValue;

            // 2) 防呆：不能把自己設為自己的父
            if (isChild && dto.CategoryId.HasValue && dto.CategoryId == dto.ParentId)
                return Json(ApiResponse<object>.Fail("ParentId 不可等於自己"));

            // 3) 新增 or 更新
            if (dto.CategoryId is null)
            {
                // (可選) 子分類先檢查父是否存在
                if (isChild && !await _db.FaqCategories.AnyAsync(c => c.FaqCategoriesId == dto.ParentId))
                    return Json(ApiResponse<object>.Fail("父分類不存在"));
                // 新增
                var baseSlug = ToSlug(dto.Name);
                var slug = await EnsureUniqueSlugAsync(baseSlug);
                var entity = new FaqCategory
                {
                    Name = dto.Name!.Trim(),
                    Slug = slug,                            // ← 不會是 NULL、也不會重複
                    ParentId = isChild ? dto.ParentId : null,
                    CreatedAt = DateTime.UtcNow
                };
                _db.FaqCategories.Add(entity);
                await _db.SaveChangesAsync();
                return Json(ApiResponse<object>.Ok(new { entity.FaqCategoriesId }, "已新增"));
            }
            else
            {
                // 更新
                var entity = await _db.FaqCategories.FindAsync(dto.CategoryId.Value);
                if (entity == null) return Json(ApiResponse<object>.Fail("找不到分類"));

                entity.Name = dto.Name!.Trim();
                entity.ParentId = isChild ? dto.ParentId : null; // 父分類 → 強制 null
                if (string.IsNullOrWhiteSpace(entity.Slug))
                    entity.Slug = await EnsureUniqueSlugAsync(ToSlug(entity.Name));
                entity.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return Json(ApiResponse<object>.Ok(new { entity.FaqCategoriesId }, "已更新"));
            }
        }
        catch (DbUpdateException ex)
        {
            // 常見：FK/唯一鍵/NOT NULL 等資料庫約束
            return Json(ApiResponse<object>.Fail($"資料庫錯誤：{ex.InnerException?.Message ?? ex.Message}"));
        }
        catch (Exception ex)
        {
            return Json(ApiResponse<object>.Fail($"伺服器錯誤：{ex.Message}"));
        }
    }

    // 刪除分類（父分類需沒有子與文章；子分類需沒有文章）
    [HttpDelete]
    [Authorize]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            // 1) 先檢查是否存在
            var cat = await _db.FaqCategories
                .FirstOrDefaultAsync(c => c.FaqCategoriesId == id);
            if (cat == null)
                return Json(ApiResponse<object>.Fail("分類不存在"));

            // 2) 是否仍有子分類
            var hasChild = await _db.FaqCategories
                .AnyAsync(c => c.ParentId == id);
            if (hasChild)
                return Json(ApiResponse<object>.Fail("仍有子分類，無法刪除"));

            // 3) 是否仍有文章
            var hasArticle = await _db.FaqArticles
                .AnyAsync(a => a.CategoryId == id);
            if (hasArticle)
                return Json(ApiResponse<object>.Fail("分類仍有文章，無法刪除"));

            // 4) 可以刪
            _db.FaqCategories.Remove(cat);
            await _db.SaveChangesAsync();
            return Json(ApiResponse<object>.Ok(new { id }, "已刪除"));
        }
        catch (DbUpdateException ex)
        {
            // FK/唯一鍵等 DB 約束錯誤：回可讀訊息（避免前端看到 HTML）
            return Json(ApiResponse<object>.Fail(
                $"資料庫錯誤：{ex.InnerException?.Message ?? ex.Message}"));
        }
        catch (Exception ex)
        {
            return Json(ApiResponse<object>.Fail($"伺服器錯誤：{ex.Message}"));
        }
    }
}
