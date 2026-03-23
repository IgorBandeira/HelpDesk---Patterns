using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Ticketing.Queries
{
    public sealed class TicketUserQueryPort : ITicketUserQueryPort
    {
        private readonly AppDbContext _db;

        public TicketUserQueryPort(AppDbContext db) => _db = db;

        public Task<bool> HasActiveTicketsAsRequesterAsync(int userId)
        {
            return _db.Tickets.AsNoTracking().AnyAsync(t =>
                t.RequesterId == userId &&
                t.Status != "Fechado" &&
                t.Status != "Cancelado"
            );
        }

        public Task<bool> HasActiveTicketsAsAssigneeAsync(int userId)
        {
            return _db.Tickets.AsNoTracking().AnyAsync(t =>
                t.AssigneeId == userId &&
                t.Status != "Fechado" &&
                t.Status != "Cancelado"
            );
        }

        public async Task<IReadOnlyList<UserTicketListItem>> ListRequestedTicketsAsync(int userId)
        {
            var items = await _db.Tickets.AsNoTracking()
                .Where(t => t.RequesterId == userId)
                .Select(t => new UserTicketListItem(t.Id, t.Title, t.Status, t.PriorityLevel))
                .ToListAsync();

            return items;
        }

        public async Task<IReadOnlyList<UserTicketListItem>> ListAssignedTicketsAsync(int userId)
        {
            var items = await _db.Tickets.AsNoTracking()
                .Where(t => t.AssigneeId == userId)
                .Select(t => new UserTicketListItem(t.Id, t.Title, t.Status, t.PriorityLevel))
                .ToListAsync();

            return items;
        }
    }
}
