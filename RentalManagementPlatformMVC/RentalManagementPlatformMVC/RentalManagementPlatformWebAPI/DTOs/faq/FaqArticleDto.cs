namespace RentalManagementPlatformWebAPI.DTOs.faq
{
    public class FaqArticleDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public int CategoryId { get; set; }
        public string? Summary { get; set; }
        public bool IsActive { get; set; } = true;
    }
    public class ArticleUpsertDto
    {
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public int CategoryId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
