using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Area.ReportForm.DTO
{
    public class OccupancyPoint
    {
        public string Date { get; set; }
        public double OccupancyRate { get; set; }
    }

    public class OccupancyPredictionResponseDto
    {
        public List<OccupancyPoint> HistoricalPoints { get; set; }
        public List<OccupancyPoint> RegressionPoints { get; set; }
    }
}
