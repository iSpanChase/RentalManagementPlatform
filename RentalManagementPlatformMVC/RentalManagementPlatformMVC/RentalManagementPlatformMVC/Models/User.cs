using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public DateTime BirthDate { get; set; }

    public string? Phone { get; set; }

    public string Address { get; set; } = null!;

    public string? ProfileImageurl { get; set; }

    public int? Point { get; set; }

    public bool Isverified { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<CouponGuest> CouponGuests { get; set; } = new List<CouponGuest>();

    public virtual ICollection<FaqArticle> FaqArticles { get; set; } = new List<FaqArticle>();

    public virtual ICollection<HostPayout> HostPayouts { get; set; } = new List<HostPayout>();

    public virtual ICollection<HostSubscription> HostSubscriptions { get; set; } = new List<HostSubscription>();

    public virtual ICollection<Message> MessageReceivers { get; set; } = new List<Message>();

    public virtual ICollection<Message> MessageSenders { get; set; } = new List<Message>();

    public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();

    public virtual ICollection<PointLedger> PointLedgers { get; set; } = new List<PointLedger>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<Review> ReviewHosts { get; set; } = new List<Review>();

    public virtual ICollection<Review> ReviewReviewers { get; set; } = new List<Review>();

    public virtual ICollection<RoomList> RoomLists { get; set; } = new List<RoomList>();

    public virtual ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();

    public virtual ICollection<UserFavoriteReport> UserFavoriteReports { get; set; } = new List<UserFavoriteReport>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
