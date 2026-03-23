using HelpDesk.Application.Ticketing.DTOs;

namespace HelpDesk.Application.Ticketing.Ports
{
    public interface ITicketSlaQueryPort
    {
        Task<IReadOnlyList<SlaMonitorTicketDto>> ListOpenTicketsWithSlaAsync(
            CancellationToken ct = default);
    }
}