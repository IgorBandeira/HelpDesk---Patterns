using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Ticketing.Queries
{
    public sealed class TicketSlaQueryPort : ITicketSlaQueryPort
    {
        private readonly AppDbContext _db;

        public TicketSlaQueryPort(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<SlaMonitorTicketDto>> ListOpenTicketsWithSlaAsync(
            CancellationToken ct = default)
        {
            return await _db.Tickets
                .AsNoTracking()
                .Where(t =>
                    t.SlaDueAt != null &&
                    t.SlaDueAt > DateTime.Now &&
                    t.Status != TicketStatus.Fechado.ToString() &&
                    t.Status != TicketStatus.Cancelado.ToString())
                .Select(t => new SlaMonitorTicketDto(
                    t.Id,
                    t.Status,
                    t.SlaStartAt,
                    t.SlaDueAt))
                .ToListAsync(ct);
        }
    }
}