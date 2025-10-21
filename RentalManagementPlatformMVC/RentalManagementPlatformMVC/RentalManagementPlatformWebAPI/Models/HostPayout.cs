using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Models;

public partial class HostPayout
{
    public int PayoutId { get; set; }

    public int? HostId { get; set; }

    public DateTime? CycleStart { get; set; }

    public DateTime? CycleEnd { get; set; }

    public decimal? AmountGross { get; set; }

    public decimal? PlatformFee { get; set; }

    public decimal? AmountNet { get; set; }

    public DateTime? PaidAt { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }
}
