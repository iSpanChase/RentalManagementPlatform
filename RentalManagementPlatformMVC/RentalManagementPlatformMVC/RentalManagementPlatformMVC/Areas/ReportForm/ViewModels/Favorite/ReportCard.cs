namespace RentalManagementPlatformMVC.Areas.ReportForm.Controllers
{
    public class ReportCard
    {
        public string? Title { get; set; }
        public string? ChartType { get; set; }
        public string? ReportId { get; set; }
        public string? Base { get; set; }
        public string? Endpoint { get; set; }
        public Dictionary<string, string>? Form { get; set; }
    }
}
