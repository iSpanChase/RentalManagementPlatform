using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class FaqArticle
{
    public int FaqArticlesId { get; set; }

    public string? Slug { get; set; }

    public int? CategoryId { get; set; }

    public int? AuthorId { get; set; }

    public string? Title { get; set; }

    public string? Summary { get; set; }

    public string? Content { get; set; }

    public bool? IsPinned { get; set; }

    public DateTime? PublishedAt { get; set; }

    public int? ViewCount { get; set; }

    public int? HelpfulYes { get; set; }

    public int? HelpfulNo { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
