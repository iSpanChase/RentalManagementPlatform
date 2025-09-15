namespace RentalManagementPlatformMVC.Areas.ReportForm.Anomaly
{
    public class AnomalyBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<AnomalyBackgroundService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

        public AnomalyBackgroundService(IServiceProvider sp, ILogger<AnomalyBackgroundService> logger)
        {
            _sp = sp; _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(Interval);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _sp.CreateScope();
                    var eval = scope.ServiceProvider.GetRequiredService<IAnomalyEvaluator>();
                    var n = await eval.EvaluateOnceAsync(stoppingToken);
                    if (n > 0) _logger.LogInformation("Anomaly events inserted: {n}", n);
                }
                catch (Exception ex) { _logger.LogError(ex, "Anomaly scan failed"); }

                await timer.WaitForNextTickAsync(stoppingToken);
            }
        }
    }
}
