using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.ReportForm.ViewModels.Query
{
    public enum TimeUnit { day, week, month, quarter, year }
    public enum ChartType { bar, line, pie }

    public abstract class ReportQueryBase
    {
        public string InstanceId { get; set; } = Guid.NewGuid().ToString("N");

        [Required]
        public TimeUnit TimeUnit { get; set; }

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
    }
}
