using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Area.ReportForm.DTO
{
    public class RevenuePoint
    {
        public string Date { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RevenuePredictionResponseDto
    {
        public List<RevenuePoint> HistoricalPoints { get; set; }
        public List<RevenuePoint> RegressionPoints { get; set; }
    }
}
