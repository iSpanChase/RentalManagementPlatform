using RentalManagementPlatformWebAPI.Data;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Entities;
using RentalManagementPlatformWebAPI.Services.Interface;

namespace RentalManagementPlatformWebAPI.Services
{
    public class MessageService : IMessageService
    {
        private readonly InMemoryStore _store;
        public MessageService(InMemoryStore store) => _store = store;

        public IEnumerable<MessageDto> GetByTicketId(Guid ticketId)
            => _store.Messages.Values
                .Where(m => m.TicketId == ticketId)
                .OrderBy(m => m.CreatedAt)
                .Select(Map);

        public MessageDto Save(Guid ticketId, string senderRole, string senderName, string content)
        {
            var msg = new Message
            {
                TicketId = ticketId,
                SenderRole = senderRole,
                SenderName = senderName,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };
            _store.Messages[msg.Id] = msg;
            return Map(msg);
        }

        private static MessageDto Map(Message m)
            => new(m.Id, m.TicketId, m.SenderRole, m.SenderName, m.Content, m.CreatedAt);
    }
}
