namespace HelpDesk.Application.Ticketing.Ports
{
    public sealed record TicketSnapshot(int Id, string Status, int? RequesterId, int? AssigneeId);

    public interface ITicketReadPort
    {
        Task<TicketSnapshot?> GetByIdAsync(int ticketId);
    }
}
