using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalManagementPlatformWebAPI.Models;

public partial class RoomList
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RoomId { get; set; }

    public int? AddressId { get; set; }

    public int? HostId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public int? MaxGuests { get; set; }

    public decimal? PricePerNight { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
