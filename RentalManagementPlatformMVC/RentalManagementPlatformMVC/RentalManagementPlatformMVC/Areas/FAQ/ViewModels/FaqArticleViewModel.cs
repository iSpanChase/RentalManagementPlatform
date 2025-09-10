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
}
