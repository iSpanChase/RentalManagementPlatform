using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Area.ReportForm.DTO
{
    public class AnalysisRequestDto
    {
        public List<int> RoomIds { get; set; } = new List<int>();
        public int Days { get; set; } = 30;
    }
}
