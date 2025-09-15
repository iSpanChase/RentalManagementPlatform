namespace RentalManagementPlatformMVC.Areas.FAQ.ViewModels
{
    public class CategoryVM
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = "";
        public int ArticleCount { get; set; }
        public int? ParentId { get; set; }

        public List<CategoryVM> Children { get; set; } = new();
    }
}
