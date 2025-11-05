using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Models;

public partial class ExternalLogin
{
    public long ExternalLoginId { get; set; }

    public int UserId { get; set; }

    public string Provider { get; set; } = null!;

    public string ProviderUserId { get; set; } = null!;

    public string? Email { get; set; }

    public string? DisplayName { get; set; }

    public string? PictureUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
