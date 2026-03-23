using HelpDesk.Application.Ticketing.DTOs;

namespace HelpDesk.Application.Ticketing.Ports
{
    public interface ITicketQueryPort
    {
        Task<TicketDetailsDto?> GetDetailsByIdAsync(int ticketId);
        Task<IReadOnlyList<TicketListItemDto>> ListAsync(
            string? status, string? priority, string? title,
            DateTime? createdFrom, DateTime? createdTo,
            int? requesterId, int? assigneeId, int? categoryId,
            DateTime? slaDueFrom, DateTime? slaDueTo,
            bool? overdueOnly, int page, int pageSize);
    }
}