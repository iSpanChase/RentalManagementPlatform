using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class FaqCategory
{
    public int FaqCategoriesId { get; set; }

    public string? Slug { get; set; }

    public int? ParentId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int? SortOrder { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<FaqArticle> FaqArticles { get; set; } = new List<FaqArticle>();

    public virtual ICollection<FaqCategory> InverseParent { get; set; } = new List<FaqCategory>();

    public virtual FaqCategory? Parent { get; set; }
}
