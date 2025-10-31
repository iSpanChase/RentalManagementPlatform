using System.Collections.Concurrent;

namespace RentalManagementPlatformWebAPI.Data
{
    public class InMemoryStore
    {
        // 併發安全
        public ConcurrentDictionary<Guid, Entities.SupportTicket> Tickets { get; } = new();
        public ConcurrentDictionary<Guid, Entities.Message> Messages { get; } = new();
    }
}
