using HelpDesk.Domain.Ticketing.Aggregates;

namespace HelpDesk.Application.Ticketing.Ports
{
    public interface ITicketRepository
    {
        Task<Ticket?> GetByIdAsync(int id);
        Task AddAsync(Ticket ticket);
        Task SaveAsync(Ticket ticket);
    }
}