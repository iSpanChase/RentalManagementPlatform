using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.FAQ.ViewModels;
using RentalManagementPlatformMVC.Data;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.FAQ.Controllers
{
    [Area("FAQ")]
    public class FaqBrowseController : Controller
    {
        private readonly RentalManagementPlatformSqlContext _db;
        public FaqBrowseController(RentalManagementPlatformSqlContext db) => _db = db;

        // GET: FAQ/FaqBrowse/Index
        public IActionResult Index()
        {
            var categories = _db.FaqCategories
                .Select(c => new CategoryVM
                {
                    CategoryId = c.FaqCategoriesId,
                    Name = c.Name ?? "(未命名)",
                    ParentId = c.ParentId
                })
                .ToList();

            // 建立樹狀結構
            var dict = categories.ToDictionary(c => c.CategoryId);
            var roots = new List<CategoryVM>();

            foreach (var c in categories)
            {
                if (c.ParentId == null)
                    roots.Add(c);
                else if (dict.TryGetValue(c.ParentId.Value, out var parent))
                    parent.Children.Add(c);
            }

            // 計算子分類文章數
            var counts = _db.FaqArticles
                .GroupBy(a => a.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToList();

            foreach (var item in counts)
            {
                if (item.CategoryId.HasValue && dict.TryGetValue(item.CategoryId.Value, out var node))
                {
                    node.ArticleCount = item.Count;
                }
            }

            return View(roots); // Index.cshtml
        }

        public IActionResult ArticlesByCategory(int id)
    {
        var articles = _db.FaqArticles
            .Where(a => a.CategoryId == id)
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.PublishedAt)
            .Select(a => new ArticleRowVM
            {
                FaqArticlesId = a.FaqArticlesId,
                Title = a.Title ?? "(未命名)",
                IsPinned = a.IsPinned,
                PublishedAt = a.PublishedAt,
                HelpfulYes = a.HelpfulYes,
                HelpfulNo = a.HelpfulNo
            })
            .ToList();

        return PartialView("_ArticlesByCategory", articles);
    }

        [HttpGet]
        public async Task<IActionResult> FeedbackByArticle(int articleId)
        {
            var items = await _db.FaqFeedbacks
                .Where(f => f.ArticleId == articleId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FeedbackVM
                {
                    FaqFeedbackId = f.FaqFeedbackId,
                    ArticleId = f.ArticleId,
                    Sentiment = f.Sentiment!,
                    Reason = f.Reason!,
                    ContactEmail = f.ContactEmail!,
                    EscalatedToTicket = f.EscalatedToTicket,
                    CreatedAt = f.CreatedAt
                })
                .AsNoTracking()
                .ToListAsync();

            ViewBag.ArticleId = articleId;
            return PartialView("_FeedbackByArticle", items);
        }
    }
}
