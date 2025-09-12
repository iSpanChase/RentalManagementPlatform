using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.FAQ.ViewModels;
using RentalManagementPlatformMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementPlatformMVC.Areas.FAQ.Controllers
{
    [Area("FAQ")]
    public class FaqArticlesController : Controller
    {
        private readonly RentalManagementPlatformSqlContext _db;
        public FaqArticlesController(RentalManagementPlatformSqlContext db) => _db = db;

        // GET: /FAQ/FaqArticles/Details/123
        [HttpGet]
        public IActionResult Details(int id)
        {
            // 文章
            var a = _db.FaqArticles
                .Where(x => x.FaqArticlesId == id)
                .Select(x => new ArticleDetailsVM
                {
                    Id = x.FaqArticlesId,
                    Title = x.Title ?? "(未命名)",
                    Summary = x.Summary,
                    Content = x.Content,
                    IsPinned = x.IsPinned,
                    PublishedAt = x.PublishedAt,
                    HelpfulYes = x.HelpfulYes,
                    HelpfulNo = x.HelpfulNo,
                    CategoryName = _db.FaqCategories
                                   .Where(c => c.FaqCategoriesId == x.CategoryId)
                                   .Select(c => c.Name)
                                   .FirstOrDefault() ?? ""
                })
                .FirstOrDefault();

            if (a == null) return NotFound();

            // 回饋（可視需要做分頁；先簡單取全部，時間新到舊）
            a.Feedbacks = _db.FaqFeedbacks
                .Where(f => f.ArticleId == id)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FeedbackVM
                {
                    FaqFeedbackId = f.FaqFeedbackId,
                    ArticleId = f.ArticleId,
                    UserId = f.UserId,
                    Sentiment = f.Sentiment ?? "no Sentiment",
                    Reason = f.Reason ?? "no reason",
                    ContactEmail = f.ContactEmail ?? "no ContactEmail",
                    EscalatedToTicket = f.EscalatedToTicket,
                    CreatedAt = f.CreatedAt
                })
                .ToList();

            return View(a); // 對應 Views/FAQ/FaqArticles/Details.cshtml
        }

        //// Index：加關鍵字/分類/分頁
        //public async Task<IActionResult> Index(string? keyword, int? categoryId, int page = 1, int size = 10)
        //{
        //    var q = _db.FaqArticles.AsQueryable();
        //    if (!string.IsNullOrWhiteSpace(keyword))
        //        q = q.Where(x => x.Title!.Contains(keyword) || x.Summary!.Contains(keyword));
        //    if (categoryId.HasValue)
        //        q = q.Where(x => x.CategoryId == categoryId);

        //    var total = await q.CountAsync();
        //    var items = await q.OrderByDescending(x => x.IsPinned)
        //                       .ThenByDescending(x => x.FaqArticlesId)
        //                       .Skip((page - 1) * size).Take(size)
        //                       .Select(x => new FaqArticleViewModel
        //                       {
        //                           FaqArticlesId = x.FaqArticlesId,
        //                           Title = x.Title!,
        //                           Summary = x.Summary,
        //                           Content = x.Content,
        //                           IsPinned = x.IsPinned,
        //                           CategoryId = x.CategoryId
        //                       }).ToListAsync();

        //    ViewBag.Total = total; ViewBag.Page = page; ViewBag.Size = size;
        //    return View(items);
        //}

        //// Create (GET)
        //public IActionResult Create() => View(new FaqArticleViewModel());

        //// Create (POST)
        //[HttpPost, ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(FaqArticleViewModel m)
        //{
        //    if (!ModelState.IsValid) return View(m);
        //    var e = new FaqArticle
        //    {
        //        Title = m.Title,
        //        Summary = m.Summary,
        //        Content = m.Content,
        //        IsPinned = m.IsPinned,
        //        CategoryId = m.CategoryId
        //    };
        //    _db.FaqArticles.Add(e);
        //    await _db.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //// Edit (GET)
        //public async Task<IActionResult> Edit(int id)
        //{
        //    var e = await _db.FaqArticles.FindAsync(id);
        //    if (e == null) return NotFound();
        //    return View(new FaqArticleViewModel
        //    {
        //        FaqArticlesId = e.FaqArticlesId,
        //        Title = e.Title!,
        //        Summary = e.Summary,
        //        Content = e.Content,
        //        IsPinned = e.IsPinned,
        //        CategoryId = e.CategoryId
        //    });
        //}

        //// Edit (POST)
        //[HttpPost, ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, FaqArticleViewModel m)
        //{
        //    if (id != m.FaqArticlesId) return BadRequest();
        //    if (!ModelState.IsValid) return View(m);

        //    var e = await _db.FaqArticles.FindAsync(id);
        //    if (e == null) return NotFound();

        //    e.Title = m.Title; e.Summary = m.Summary; e.Content = m.Content;
        //    e.IsPinned = m.IsPinned; e.CategoryId = m.CategoryId;
        //    await _db.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //// GET: FAQ/FaqArticles/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var faqArticle = await _db.FaqArticles
        //        .FirstOrDefaultAsync(m => m.FaqArticlesId == id);
        //    if (faqArticle == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(faqArticle);
        //}

        //// POST: FAQ/FaqArticles/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var faqArticle = await _db.FaqArticles.FindAsync(id);
        //    if (faqArticle != null)
        //    {
        //        _db.FaqArticles.Remove(faqArticle);
        //    }

        //    await _db.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool FaqArticleExists(int id)
        {
            return _db.FaqArticles.Any(e => e.FaqArticlesId == id);
        }
    }
}
