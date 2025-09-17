using System;
using System.Collections.Generic;

namespace AutoFK.Models;

public partial class PaymentTransaction
{
    public int TransactionId { get; set; }

    public int? PaymentId { get; set; }

    public string? ProviderTxnId { get; set; }

    public string? ResponseCode { get; set; }

    public string? Provider { get; set; }

    public string? ResponseMessage { get; set; }

    public string? TxnRef { get; set; }

    public DateTime? CreatedAt { get; set; }
}
