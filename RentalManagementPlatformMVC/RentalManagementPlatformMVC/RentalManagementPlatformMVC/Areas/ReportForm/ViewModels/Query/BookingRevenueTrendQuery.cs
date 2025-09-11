namespace RentalManagementPlatformMVC.Areas.ReportForm.ViewModels.Query
{
    public class BookingRevenueTrendQuery : ReportQueryBase
    {
        public ChartType ChartType { get; set; } = ChartType.bar;
        public int? CityId = null;
        public int? DistrictId = null;
    }
}
