using Microsoft.AspNetCore.SignalR;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using System.Text.RegularExpressions;

namespace RentalManagementPlatformWebAPI.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IMessageService _messages;
        private readonly ISupportTicketService _tickets;

        public ChatHub(IMessageService messages, ISupportTicketService tickets)
        {
            _messages = messages;
            _tickets = tickets;
        }

        public async Task JoinRoom(string ticketId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ticketId);
        }

        public async Task LeaveRoom(string ticketId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticketId);
        }

        public async Task SendMessage(string ticketId, string senderRole, string senderName, string content)
        {
            if (!Guid.TryParse(ticketId, out var tid) || !_tickets.Exists(tid))
                throw new HubException("Ticket not found");

            var dto = _messages.Save(tid, senderRole, senderName, content);
            await Clients.Group(ticketId).SendAsync("ReceiveMessage", dto);
        }
    }
}
