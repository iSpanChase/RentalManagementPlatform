namespace RentalManagementPlatformMVC.Areas.ReportForm.Anomaly
{
    public class AnomalyBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);

        public AnomalyBackgroundService(IServiceProvider sp)
        {
            _sp = sp;
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
                }
                catch (Exception ex) {}

                await timer.WaitForNextTickAsync(stoppingToken);
            }
        }
    }
}
