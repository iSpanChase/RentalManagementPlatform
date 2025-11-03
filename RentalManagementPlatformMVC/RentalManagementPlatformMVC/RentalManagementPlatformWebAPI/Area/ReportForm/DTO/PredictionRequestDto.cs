using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Area.ReportForm.DTO
{
    public class PredictionRequestDto
    {
        public List<int> RoomIds { get; set; }
        public int ForecastDays { get; set; }
    }
}
