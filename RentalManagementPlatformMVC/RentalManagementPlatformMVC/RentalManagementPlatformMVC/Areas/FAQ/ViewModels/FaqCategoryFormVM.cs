using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RentalManagementPlatformMVC.Areas.FAQ.ViewModels
{
    public class FaqCategoryFormVM
    {
        public int? FaqCategoriesId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "分類名稱")]
        public string Name { get; set; } = "";

        [Display(Name = "父分類")]
        public int? ParentId { get; set; }

        // 下拉用
        public IEnumerable<SelectListItem>? ParentOptions { get; set; }
    }

    public class CategoryListRowVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? ParentName { get; set; }
        public int ArticleCount { get; set; }
    }

    public class FaqCategoryDeleteVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool HasChildren { get; set; }
        public bool HasArticles { get; set; }
    }
}
