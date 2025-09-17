using System.Collections.Concurrent;

namespace RentalManagementPlatformMVC.Areas.ReportForm.Anomaly
{
    public class AnomalyNotifier
    {
        private readonly ConcurrentDictionary<Guid, Func<string, Task>> _subs = new();

        public Guid Subscribe(Func<string, Task> onEvent)
        {
            var id = Guid.NewGuid();
            _subs[id] = onEvent;
            return id;
        }

        public void Unsubscribe(Guid id) => _subs.TryRemove(id, out _);

        public Task BroadcastAsync(string json)
        {
            var tasks = _subs.Values.Select(cb => SafeInvoke(cb, json));
            return Task.WhenAll(tasks);
        }

        private static async Task SafeInvoke(Func<string, Task> cb, string payload)
        {
            try { await cb(payload); } catch { /* 忽略個別連線錯誤 */ }
        }
    }
}
