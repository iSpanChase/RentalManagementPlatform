namespace RentalManagementPlatformMVC.Areas.ReportForm.ViewModels.Query
{
    public class BookingAmountTrendQuery : ReportQueryBase
    {
        public ChartType ChartType { get; set; } = ChartType.bar;
        public int? CityId = null;
        public int? DistrictId = null;
    }
}
