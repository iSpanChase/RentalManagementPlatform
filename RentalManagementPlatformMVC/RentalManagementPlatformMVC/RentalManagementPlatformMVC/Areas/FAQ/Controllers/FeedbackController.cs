using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.FAQ.Controllers;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatform.Areas.FAQ.Controllers;

[Area("FAQ")]
[Authorize]
[Route("FAQ/Admin/[controller]/[action]")]
public class FeedbackController : Controller
{
    private readonly RentalManagementPlatformSqlContext _db;
    public FeedbackController(RentalManagementPlatformSqlContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(int? articleId)
    {
        var q = _db.FaqFeedbacks.Include(f => f.Article).AsQueryable();
        if (articleId.HasValue) q = q.Where(f => f.ArticleId == articleId.Value);

        var data = await q.OrderByDescending(f => f.FaqFeedbackId)
            .Select(f => new {
                f.FaqFeedbackId,
                f.ArticleId,
                ArticleTitle = f.Article != null ? f.Article.Title : null,
                f.UserId,
                f.Sentiment,
                f.Reason,
                f.ContactEmail,
                f.CreatedAt
            }).ToListAsync();

        return Json(ApiResponse<object>.Ok(data));
    }

    [HttpDelete]
    [Authorize]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var f = await _db.FaqFeedbacks.FindAsync(id);
        if (f == null) return Json(ApiResponse<object>.Fail("回饋不存在"));
        _db.FaqFeedbacks.Remove(f);
        await _db.SaveChangesAsync();
        return Json(ApiResponse<object>.Ok(new { id }, "已刪除"));
    }

    // 提供 Feedback 篩選下拉：全部文章
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> AllArticlesForDropdown()
    {
        var data = await _db.FaqArticles
            .OrderByDescending(a => a.FaqArticlesId)
            .Select(a => new { ArticleId = a.FaqArticlesId, Title = a.Title })
            .ToListAsync();
        return Json(ApiResponse<object>.Ok(data));
    }
}
