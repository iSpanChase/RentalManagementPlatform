using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class SupportTicket
{
    public int SupportTicketsId { get; set; }

    public int? RelatedFeedbackId { get; set; }

    public int? CreatedByUserId { get; set; }

    public int? AssignedStaffId { get; set; }

    public string? Subject { get; set; }

    public string? Content { get; set; }

    public string? ContactEmail { get; set; }

    public string? Priority { get; set; }

    public string? Source { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? AssignedStaff { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual FaqArticle? RelatedFeedback { get; set; }
}
