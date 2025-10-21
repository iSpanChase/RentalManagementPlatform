using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string Gender { get; set; } = null!;

    public DateTime BirthDate { get; set; }

    public string? Phone { get; set; }

    public string Address { get; set; } = null!;

    public string? ProfileImageurl { get; set; }

    public int? Point { get; set; }

    public bool Isverified { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string Provider { get; set; } = null!;

    public string? ProviderSubject { get; set; }

    public DateTime? LastLoginAt { get; set; }
}
