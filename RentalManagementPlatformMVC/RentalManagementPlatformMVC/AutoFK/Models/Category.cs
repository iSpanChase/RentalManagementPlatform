using System;
using System.Collections.Generic;

namespace AutoFK.Models;

public partial class Category
{
    public int CategoriesId { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }
}
