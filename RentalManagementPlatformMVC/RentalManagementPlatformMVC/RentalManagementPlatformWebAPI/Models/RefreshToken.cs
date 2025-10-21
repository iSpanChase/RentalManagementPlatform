using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Models;

public partial class RefreshToken
{
    public Guid RefreshTokenId { get; set; }

    public int UserId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool Revoked { get; set; }

    public DateTime CreatedAt { get; set; }
}
