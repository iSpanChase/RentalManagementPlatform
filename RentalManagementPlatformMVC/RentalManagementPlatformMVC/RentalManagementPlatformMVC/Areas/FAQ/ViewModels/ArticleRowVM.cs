namespace RentalManagementPlatformMVC.Areas.FAQ.ViewModels
{
    public class ArticleRowVM
    {
        public int FaqArticlesId { get; set; }
        public string Title { get; set; } = "";     // ← 可空
        public bool? IsPinned { get; set; }        // ← 可空
        public DateTime? PublishedAt { get; set; } // ← 可空
        public int? ViewCount { get; set; }        // ← 可空
        public int? HelpfulYes { get; set; }       // ← 可空
        public int? HelpfulNo { get; set; }        // ← 可空
        public int? CategoryId { get; set; }       // ← 可空
    }
}
