using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Models;

public partial class FaqFeedback
{
    public int FaqFeedbackId { get; set; }

    public int ArticleId { get; set; }

    public int? UserId { get; set; }

    public string? Sentiment { get; set; }

    public string? Reason { get; set; }

    public string? ContactEmail { get; set; }

    public bool? EscalatedToTicket { get; set; }

    public DateTime? CreatedAt { get; set; }
}
