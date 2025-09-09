using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.FAQ.Controllers
{
    [Area("FAQ")]
    public class FaqArticlesController : Controller
    {
        private readonly RentalManagementPlatformSqlContext _context;

        public FaqArticlesController(RentalManagementPlatformSqlContext context)
        {
            _context = context;
        }

        // GET: FAQ/FaqArticles
        public async Task<IActionResult> Index()
        {
            return View(await _context.FaqArticles.ToListAsync());
        }

        // GET: FAQ/FaqArticles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faqArticle = await _context.FaqArticles
                .FirstOrDefaultAsync(m => m.FaqArticlesId == id);
            if (faqArticle == null)
            {
                return NotFound();
            }

            return View(faqArticle);
        }

        // GET: FAQ/FaqArticles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FAQ/FaqArticles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FaqArticlesId,Slug,CategoryId,AuthorId,Title,Summary,Content,IsPinned,PublishedAt,ViewCount,HelpfulYes,HelpfulNo,Status,CreatedAt,UpdatedAt")] FaqArticle faqArticle)
        {
            if (ModelState.IsValid)
            {
                _context.Add(faqArticle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(faqArticle);
        }

        // GET: FAQ/FaqArticles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faqArticle = await _context.FaqArticles.FindAsync(id);
            if (faqArticle == null)
            {
                return NotFound();
            }
            return View(faqArticle);
        }

        // POST: FAQ/FaqArticles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FaqArticlesId,Slug,CategoryId,AuthorId,Title,Summary,Content,IsPinned,PublishedAt,ViewCount,HelpfulYes,HelpfulNo,Status,CreatedAt,UpdatedAt")] FaqArticle faqArticle)
        {
            if (id != faqArticle.FaqArticlesId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(faqArticle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FaqArticleExists(faqArticle.FaqArticlesId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(faqArticle);
        }

        // GET: FAQ/FaqArticles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faqArticle = await _context.FaqArticles
                .FirstOrDefaultAsync(m => m.FaqArticlesId == id);
            if (faqArticle == null)
            {
                return NotFound();
            }

            return View(faqArticle);
        }

        // POST: FAQ/FaqArticles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var faqArticle = await _context.FaqArticles.FindAsync(id);
            if (faqArticle != null)
            {
                _context.FaqArticles.Remove(faqArticle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FaqArticleExists(int id)
        {
            return _context.FaqArticles.Any(e => e.FaqArticlesId == id);
        }
    }
}
