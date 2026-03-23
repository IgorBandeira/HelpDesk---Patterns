using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Ticketing.Queries
{
    public sealed class TicketCategoryQueryPort : ITicketCategoryQueryPort
    {
        private readonly AppDbContext _db;

        public TicketCategoryQueryPort(AppDbContext db) => _db = db;

        public async Task<bool> HasActiveTicketsForCategoryAsync(int categoryId)
        {
            return await _db.Set<Models.TicketEntity>()
                .AnyAsync(x =>
                    x.CategoryId == categoryId &&
                    x.Status != TicketStatus.Fechado.ToString() &&
                    x.Status != TicketStatus.Cancelado.ToString());
        }
    }
}
