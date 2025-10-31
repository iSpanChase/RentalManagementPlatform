using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Area.ReportForm.DTO.Report
{
    public class RevenueKpiRequestDto
    {
        public List<int> RoomIds { get; set; } = new List<int>();
        public int Days { get; set; } = 30;
    }
}
