using System;
using System.Collections.Generic;

namespace AutoFK.Models;

public partial class SupportTicket
{
    public int SupportTicketsId { get; set; }

    public int? RelatedArticleId { get; set; }

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
}
