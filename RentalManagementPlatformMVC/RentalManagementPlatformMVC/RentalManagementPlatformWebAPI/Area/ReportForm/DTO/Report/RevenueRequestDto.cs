using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Area.ReportForm.DTO.Report
{
    public class RevenueRequestDto
    {
        public List<int> RoomIds { get; set; } = new List<int>();
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; }
        public string GroupBy { get; set; } = "day"; // day, week, month
    }
}
