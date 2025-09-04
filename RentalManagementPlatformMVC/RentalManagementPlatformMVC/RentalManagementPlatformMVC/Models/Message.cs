using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public int? TicketId { get; set; }

    public int? BookingId { get; set; }

    public int? ReceiverId { get; set; }

    public int? RoomId { get; set; }

    public int? SenderId { get; set; }

    public int? FaqId { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }
}
