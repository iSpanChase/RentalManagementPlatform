using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? BookingId { get; set; }

    public decimal? Amount { get; set; }

    public string? Method { get; set; }

    public DateTime? PaidAt { get; set; }

    public string? PaymentRef { get; set; }

    public string? OrderNumberSnapshot { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
}
