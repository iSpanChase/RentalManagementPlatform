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

        public async Task<IActionResult> Index()
        {
            var categories = await _db.FaqCategories
                .Select(c => new CategoryVM
                {
                    CategoryId = c.FaqCategoriesId,
                    Name = c.Name,
                    ArticleCount = _db.FaqArticles.Count(a => a.CategoryId == c.FaqCategoriesId)
                })
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> ArticlesByCategory(int id)
        {
            var rows = await _db.FaqArticles
                .Where(a => a.CategoryId == id)
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.PublishedAt)
                .Select(a => new ArticleRowVM
                {
                    FaqArticlesId = a.FaqArticlesId,
                    Title = a.Title ?? "(未命名)",
                    IsPinned = a.IsPinned,
                    PublishedAt = a.PublishedAt,
                    ViewCount = a.ViewCount,
                    HelpfulYes = a.HelpfulYes,
                    HelpfulNo = a.HelpfulNo,
                    CategoryId = a.CategoryId
                })
                .AsNoTracking()
                .ToListAsync();

            return PartialView("_ArticlesByCategory", rows);
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
