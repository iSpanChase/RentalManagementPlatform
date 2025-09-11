namespace RentalManagementPlatformMVC.Areas.ReportForm.ViewModels
{
    public enum ChartType { bar, line, pie }
    public enum TimeUnit { day, week, month, quarter, year }

    public class ReportChartCardViewModel
    {
        public string InstanceId { get; set; } = Guid.NewGuid().ToString("N");
        public ChartType ChartType { get; set; } = ChartType.bar;
        public TimeUnit TimeUnit { get; set; } = TimeUnit.day;

        // 預設時間或選單值（可選）
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
