using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Ticketing.Queries
{
    public sealed class TicketReadPort : ITicketReadPort
    {
        private readonly AppDbContext _db;

        public TicketReadPort(AppDbContext db) => _db = db;

        public async Task<TicketSnapshot?> GetByIdAsync(int ticketId)
        {
            return await _db.Tickets
                .AsNoTracking()
                .Where(x => x.Id == ticketId)
                .Select(x => new TicketSnapshot(
                    x.Id,
                    x.Status,
                    x.RequesterId,
                    x.AssigneeId))
                .FirstOrDefaultAsync();
        }
    }
}