using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class PostsCategory
{
    public int PostCategoriesId { get; set; }

    public int PostsId { get; set; }

    public int CategoriesId { get; set; }

    public virtual Category Categories { get; set; } = null!;

    public virtual Post Posts { get; set; } = null!;
}
