namespace RentalManagementPlatformWebAPI.Area.ReportForm.DTO
{
    public class OccupancyDataPoint
    {
        public string date { get; set; } // yyyy-MM-dd
        public decimal occupancyRate { get; set; } // Percentage
    }
}