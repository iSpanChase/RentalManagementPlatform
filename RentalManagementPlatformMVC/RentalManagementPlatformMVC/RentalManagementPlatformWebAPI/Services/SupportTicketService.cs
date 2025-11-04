using RentalManagementPlatformWebAPI.Data;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Entities;
using RentalManagementPlatformWebAPI.Services.Interfaces;

namespace RentalManagementPlatformWebAPI.Services
{
    public class SupportTicketService : ISupportTicketService
    {
        private readonly InMemoryStore _store;
        public SupportTicketService(InMemoryStore store) => _store = store;

        public TicketDto Create(string title, string userName)
        {
            var t = new SupportTicket { Title = title, UserName = userName };
            _store.Tickets[t.Id] = t;
            return Map(t);
        }

        public IEnumerable<TicketDto> List(string? status)
        {
            var q = _store.Tickets.Values.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status);
            return q.OrderByDescending(x => x.UpdatedAt).Select(Map);
        }

        public bool Close(Guid id)
        {
            if (!_store.Tickets.TryGetValue(id, out var t)) return false;
            t.Status = "closed";
            t.UpdatedAt = DateTime.UtcNow;
            return true;
        }

        public bool Exists(Guid id) => _store.Tickets.ContainsKey(id);

        private static TicketDto Map(SupportTicket t)
            => new(t.Id, t.Title, t.UserName, t.Status, t.CreatedAt, t.UpdatedAt);
    }
}
