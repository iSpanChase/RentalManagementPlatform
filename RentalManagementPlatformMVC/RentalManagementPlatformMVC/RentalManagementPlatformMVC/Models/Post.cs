using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class Post
{
    public int PostsId { get; set; }

    public int UserId { get; set; }

    public int RegionId { get; set; }

    public string Title { get; set; } = null!;

    public string ContactName { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? Address { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactNote { get; set; }

    public DateTime? PublishAt { get; set; }

    public DateTime? ExpireAt { get; set; }

    public int? Views { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public decimal? ProposedPrice { get; set; }

    public virtual ICollection<PostsCategory> PostsCategories { get; set; } = new List<PostsCategory>();

    public virtual District Region { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
