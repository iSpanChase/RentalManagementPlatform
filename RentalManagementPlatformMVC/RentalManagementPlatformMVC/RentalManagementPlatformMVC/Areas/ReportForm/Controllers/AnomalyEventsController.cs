using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.ReportForm.Anomaly;

namespace RentalManagementPlatformMVC.Areas.ReportForm.Controllers
{
    [Area("ReportForm")]
    [Authorize] // 如果你要限定登入期間才接收
    public class AnomalyEventsController : Controller
    {
        private readonly AnomalyNotifier _notifier;
        public AnomalyEventsController(AnomalyNotifier notifier) => _notifier = notifier;

        // GET /ReportForm/AnomalyEvents/Stream
        [HttpGet]
        public async Task Stream(CancellationToken ct)
        {
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("X-Accel-Buffering", "no"); // Nginx 如有反代，避免緩衝
            await Response.Body.FlushAsync(ct);

            // 訂閱：每當有事件要廣播，就寫一行 "data: ...\n\n"
            var subId = _notifier.Subscribe(async json =>
            {
                await Response.WriteAsync($"data: {json}\n\n", ct);
                await Response.Body.FlushAsync(ct);
            });

            try
            {
                // 讓連線維持到客戶端關閉或頁面離開
                await Task.Delay(Timeout.Infinite, ct);
            }
            catch (OperationCanceledException) { }
            finally
            {
                _notifier.Unsubscribe(subId);
            }
        }
    }
}
