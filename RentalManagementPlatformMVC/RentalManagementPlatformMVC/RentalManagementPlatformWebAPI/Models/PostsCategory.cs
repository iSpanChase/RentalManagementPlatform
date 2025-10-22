using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Models;

public partial class PostsCategory
{
    public int PostCategoriesId { get; set; }

    public int PostsId { get; set; }

    public int CategoriesId { get; set; }
}
