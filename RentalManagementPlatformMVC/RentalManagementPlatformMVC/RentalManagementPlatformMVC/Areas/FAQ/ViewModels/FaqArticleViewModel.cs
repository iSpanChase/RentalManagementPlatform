using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.FAQ.ViewModels
{
    public class FaqArticleViewModel
    {
        public int FaqArticlesId { get; set; }

        [Required, StringLength(255)]
        public string Title { get; set; } = "";

        [StringLength(255)]
        public string? Summary { get; set; }

        [StringLength(512)]
        public string? Content { get; set; }

        public bool IsPinned { get; set; }
        public int? CategoryId { get; set; }
    }
    public class ArticleDetailsVM
    {
        // 文章基本資料
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Summary { get; set; }
        public string? Content { get; set; }
        public bool? IsPinned { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int? HelpfulYes { get; set; }
        public int? HelpfulNo { get; set; }

        // 額外資訊
        public string CategoryName { get; set; } = "";
        public int FeedbackCount => Feedbacks?.Count ?? 0;

        // 該文章的回饋列表
        public List<FeedbackVM> Feedbacks { get; set; } = new();
    }
}
