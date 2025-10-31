namespace RentalManagementPlatformWebAPI.DTOs.faq
{
    public class CategoryTreeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public bool IsActive { get; set; } = true;
        public List<CategoryTreeDto> Children { get; set; } = new();
        public List<FaqArticleDto> Articles { get; set; } = new();
    }

    public class CategoryUpsertDto
    {
        public string Name { get; set; } = default!;
        public int? ParentId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
