namespace RentalManagementPlatformMVC.Areas.ReportForm.AnomalyEvaluator
{
    public interface IAnomalyEvaluator
    {
        Task<int> EvaluateOnceAsync(CancellationToken ct = default);
    }
}
