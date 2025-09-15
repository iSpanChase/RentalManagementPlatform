namespace RentalManagementPlatformMVC.Areas.ReportForm.Anomaly
{
    public interface IAnomalyEvaluator
    {
        Task<int> EvaluateOnceAsync(CancellationToken ct = default);
    }
}
