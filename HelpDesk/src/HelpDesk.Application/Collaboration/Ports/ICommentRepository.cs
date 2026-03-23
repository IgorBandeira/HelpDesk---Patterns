using HelpDesk.Domain.Collaboration.Aggregates;

namespace HelpDesk.Application.Collaboration.Ports
{
    public interface ICommentRepository
    {
        Task<TicketComment?> GetByIdAsync(int ticketId, int commentId);
        Task AddAsync(TicketComment comment);
        Task SaveAsync(TicketComment comment);
        Task DeleteAsync(TicketComment comment);

        Task<IReadOnlyList<TicketComment>> ListByTicketAsync(int ticketId);
    }
}
