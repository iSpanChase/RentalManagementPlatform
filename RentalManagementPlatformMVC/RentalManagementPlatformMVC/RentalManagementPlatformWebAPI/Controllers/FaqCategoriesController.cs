using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.DTOs.faq;
using RentalManagementPlatformWebAPI.Models;
using System;

namespace RentalManagementPlatformWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FaqCategoriesController : ControllerBase
    {
        private readonly RentalManagementPlatformSqlContext _db;
        public FaqCategoriesController(RentalManagementPlatformSqlContext db) => _db = db;

        /// <summary>取得樹狀分類（可選是否帶文章）</summary>
        [HttpGet("tree")]
        public async Task<ActionResult<IEnumerable<CategoryTreeDto>>> GetTree([FromQuery] bool includeArticles = true)
        {
            // 讀平面分類
            var cats = await _db.FaqCategories
                .AsNoTracking()
                .OrderBy(c => c.SortOrder).ThenBy(c => c.FaqCategoriesId)
                .Select(c => new
                {
                    Id = c.FaqCategoriesId,  // ← 取別名
                    c.Name,
                    c.IsActive,         // 若是 bool? 可用 (c.IsActive ?? true)
                    c.ParentId
                })
                .ToListAsync();

            // 文章對應（可選）
            var articlesLookup = await _db.FaqArticles
            .AsNoTracking()
            .Where(a => a.CategoryId.HasValue)           // 先排除 null
            .OrderBy(a => a.FaqArticlesId)
            .GroupBy(a => a.CategoryId!.Value)           // 這裡用 Value 變成 int
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Select(a => new FaqArticleDto
                {
                    Id = a.FaqArticlesId,
                    Title = a.Title,
                    Content = a.Content,
                    CategoryId = a.CategoryId!.Value,    // DTO 是 int 就給 Value
                    //IsActive = a.IsActive
                }).ToList()
            );

            // 組樹
            var map = cats.ToDictionary(
                c => c.Id,
                c => new CategoryTreeDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsActive = c.IsActive ?? true,
                    Children = new(),
                    Articles = articlesLookup.ContainsKey(c.Id) ? articlesLookup[c.Id] : new()
                });

            List<CategoryTreeDto> roots = new();
            foreach (var c in cats)
            {
                if (c.ParentId is null) roots.Add(map[c.Id]);
                else if (map.ContainsKey(c.ParentId.Value)) map[c.ParentId.Value].Children.Add(map[c.Id]);
            }
            return Ok(roots);
        }

        /// <summary>取得分類下文章（清單）</summary>
        [HttpGet("{id:int}/articles")]
        public async Task<ActionResult<IEnumerable<FaqArticleDto>>> GetArticlesByCategory(int id)
        {
            var exists = await _db.FaqCategories.AsNoTracking().AnyAsync(x => x.FaqCategoriesId == id);
            if (!exists) return NotFound();

            var items = await _db.FaqArticles.AsNoTracking()
                .Where(a => a.CategoryId == id)
                .OrderByDescending(a => a.FaqArticlesId)
                .Select(a => new FaqArticleDto
                {
                    Id = a.FaqArticlesId,               // ← 正確主鍵
                    Title = a.Title,
                    Content = a.Content,
                    CategoryId = a.CategoryId!.Value,           // ← 這裡用 !.Value；因為上面 Where 已保證等於 id
                    //
                    //IsActive = a.IsActive
                }).ToListAsync();

            return Ok(items);
        }
    }
}
