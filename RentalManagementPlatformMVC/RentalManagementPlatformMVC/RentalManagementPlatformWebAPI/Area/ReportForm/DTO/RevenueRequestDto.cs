namespace RentalManagementPlatformWebAPI.Area.ReportForm.DTO
{
    public class RevenueRequestDto
    {
        public Guid PropertyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string GroupBy { get; set; } = "day"; // day, week, month
    }
}
